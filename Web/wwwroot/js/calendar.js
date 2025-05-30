// Calendar functionality using FullCalendar
class CalendarApp {
    constructor() {
        this.calendar = null;
        this.events = [];
        this.currentEvent = null;
        this.isReadOnly = false;
        this.currentUserEmail = window.currentUserEmail || '';
        this.init();
    }

    init() {
        this.bindEvents();
        this.initializeCalendar();
        this.loadEvents();
        
        // Set initial statistics title after calendar is rendered
        setTimeout(() => {
            if (this.calendar) {
                const view = this.calendar.view;
                this.updateStatisticsTitle(view.type, view.activeStart, view.activeEnd);
            }
        }, 100);
    }

    bindEvents() {
        // Modal events
        document.getElementById('close-modal').addEventListener('click', () => this.closeModal());
        document.getElementById('cancel-btn').addEventListener('click', () => this.closeModal());
        document.getElementById('event-form').addEventListener('submit', (e) => this.handleFormSubmit(e));
        document.getElementById('delete-event-btn').addEventListener('click', () => this.handleDelete());
        document.getElementById('retry-btn').addEventListener('click', () => this.loadEvents());

        // Close modal on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && !document.getElementById('event-modal').classList.contains('hidden')) {
                this.closeModal();
            }
        });

        // Close modal when clicking outside
        document.getElementById('event-modal').addEventListener('click', (e) => {
            if (e.target.id === 'event-modal') {
                this.closeModal();
            }
        });
    }

    initializeCalendar() {
        const calendarEl = document.getElementById('calendar');
        
        this.calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'timeGridWeek',
            locale: 'es',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay'
            },
            height: 'auto',
            editable: true,
            selectable: true,
            selectMirror: true,
            dayMaxEvents: true,
            weekends: true,
            businessHours: {
                daysOfWeek: [1, 2, 3, 4, 5], // Monday - Friday
                startTime: '07:00',
                endTime: '13:00',
            },
            slotMinTime: '07:00:00',
            slotMaxTime: '13:00:00',
            events: [],
            
            // Event handlers
            select: (info) => this.handleDateSelect(info),
            eventClick: (info) => this.handleEventClick(info),
            eventDrop: (info) => this.handleEventDrop(info),
            eventResize: (info) => this.handleEventResize(info),
            datesSet: (info) => this.handleDatesSet(info)
        });

        this.calendar.render();
    }

    async loadEvents() {
        try {
            this.showLoading();
            
            const response = await fetch('/Calendar/GetEvents', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            const result = await response.json();
            
            if (result.success && result.data) {
                this.events = result.data;
                console.log('Loaded events:', this.events.length, 'events');
                console.log('Sample events:', this.events.slice(0, 3));
                
                this.calendar.removeAllEvents();
                
                // Convert events to FullCalendar format
                const calendarEvents = this.events.map(event => ({
                    id: event.id,
                    title: event.title,
                    start: this.parseDateTime(event.start),
                    end: this.parseDateTime(event.end),
                    description: event.description,
                    location: event.location,
                    email: event.email,
                    backgroundColor: event.isCancelled ? '#dc2626' : (event.isConfirmed ? '#16a34a' : '#3b82f6'),
                    borderColor: event.isCancelled ? '#dc2626' : (event.isConfirmed ? '#16a34a' : '#3b82f6')
                }));
                
                this.calendar.addEventSource(calendarEvents);
                this.showCalendar();
            } else {
                this.showError(result.message || 'Error loading events');
            }
        } catch (error) {
            console.error('Error loading events:', error);
            this.showError('Error loading events. Please try again.');
        }
    }

    handleDateSelect(info) {
        // Create new event on date selection
        const startDate = new Date(info.start);
        const endDate = new Date(info.start);
        
        // If it's an all-day selection, set specific times
        if (info.allDay) {
            startDate.setHours(9, 0, 0);
            endDate.setHours(10, 0, 0);
        } else {
            endDate.setTime(startDate.getTime() + (60 * 60 * 1000)); // Add 1 hour
        }

        this.openModal({
            start: startDate,
            end: endDate
        });

        // Clear the selection
        this.calendar.unselect();
    }

    handleEventClick(info) {
        const event = this.events.find(e => e.id === info.event.id);
        if (event) {
            // Check if event is in the past
            const endDate = new Date(this.parseDateTime(event.end));
            const isPastEvent = endDate < new Date();
            
            this.openModal(event, isPastEvent);
        }
    }

    async handleEventDrop(info) {
        await this.updateEventTimes(info.event, info.event.start, info.event.end);
    }

    async handleEventResize(info) {
        await this.updateEventTimes(info.event, info.event.start, info.event.end);
    }

    async updateEventTimes(calendarEvent, newStart, newEnd) {
        try {
            const eventData = {
                id: parseInt(calendarEvent.id),
                title: calendarEvent.title,
                start: this.formatDateTimeForServer(newStart),
                end: this.formatDateTimeForServer(newEnd),
                description: calendarEvent.extendedProps.description,
                location: calendarEvent.extendedProps.location
            };

            const response = await fetch('/Calendar/UpdateEvent', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(eventData)
            });

            const result = await response.json();
            
            if (result.success) {
                this.showToast('Evento actualizado correctamente', 'success');
                await this.loadEvents();
                // Update dashboard statistics after event update
                this.refreshDashboardStatistics();
            } else {
                this.showToast(result.message || 'Error updating event', 'error');
                calendarEvent.revert();
            }
        } catch (error) {
            console.error('Error updating event:', error);
            this.showToast('Error updating event', 'error');
            calendarEvent.revert();
        }
    }

    handleDatesSet(info) {
        // This is called when the calendar view changes
        console.log('Date range changed:', info.start, 'to', info.end);
        
        // Update dashboard statistics for the current view
        this.updateDashboardStatistics(info.start, info.end);
        
        // Update statistics title based on calendar view
        this.updateStatisticsTitle(info.view.type, info.start, info.end);
    }

    openModal(event = null, readOnly = false) {
        this.currentEvent = event;
        this.isReadOnly = readOnly;
        
        const modal = document.getElementById('event-modal');
        const form = document.getElementById('event-form');
        const title = document.getElementById('modal-title');
        const deleteBtn = document.getElementById('delete-event-btn');
        const saveBtn = document.getElementById('save-btn');
        
        // Reset form
        form.reset();
        
        if (event && event.id) {
            // Edit mode
            const baseTitle = readOnly ? 'View Event' : 'Edit Event';
            const emailPart = event.email ? ` - <span style="color: #3b82f6; font-weight: 500;">${event.email}</span>` : '';
            title.innerHTML = baseTitle + emailPart;
            document.getElementById('event-id').value = event.id;
            document.getElementById('event-title').value = event.title || '';
            document.getElementById('event-description').value = event.description || '';
            document.getElementById('event-location').value = event.location || '';
            document.getElementById('event-start').value = this.formatDateTimeForInput(this.parseDateTime(event.start));
            document.getElementById('event-end').value = this.formatDateTimeForInput(this.parseDateTime(event.end));
            deleteBtn.classList.remove('hidden');
        } else {
            // Create mode
            const userEmailPart = this.currentUserEmail ? ` - <span style="color: #3b82f6; font-weight: 500;">${this.currentUserEmail}</span>` : '';
            title.innerHTML = 'Create Event' + userEmailPart;
            document.getElementById('event-id').value = '';
            if (event) {
                document.getElementById('event-start').value = this.formatDateTimeForInput(event.start);
                document.getElementById('event-end').value = this.formatDateTimeForInput(event.end);
            }
            deleteBtn.classList.add('hidden');
        }
        
        // Handle read-only mode
        const inputs = form.querySelectorAll('input, textarea');
        inputs.forEach(input => {
            input.disabled = readOnly;
        });
        
        if (readOnly) {
            saveBtn.style.display = 'none';
            deleteBtn.style.display = 'none';
            document.getElementById('cancel-btn').textContent = 'Close';
        } else {
            saveBtn.style.display = 'block';
            document.getElementById('cancel-btn').textContent = 'Cancel';
        }
        
        modal.classList.remove('hidden');
        document.getElementById('event-title').focus();
    }

    closeModal() {
        document.getElementById('event-modal').classList.add('hidden');
        this.currentEvent = null;
        this.isReadOnly = false;
    }

    async handleFormSubmit(e) {
        e.preventDefault();
        
        if (this.isReadOnly) return;
        
        const formData = new FormData(e.target);
        const eventData = {
            title: formData.get('title'),
            description: formData.get('description'),
            start: formData.get('start').split('T')[0],
            end: formData.get('end').split('T')[0],
            location: formData.get('location')
        };
        
        try {
            let response;
            
            if (this.currentEvent && this.currentEvent.id) {
                // Update existing event
                eventData.id = parseInt(this.currentEvent.id);
                response = await fetch('/Calendar/UpdateEvent', {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(eventData)
                });
            } else {
                // Create new event
                response = await fetch('/Calendar/CreateEvent', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(eventData)
                });
            }
            
            const result = await response.json();
            
            if (result.success) {
                this.showToast(this.currentEvent && this.currentEvent.id ? 'Evento actualizado correctamente' : 'Evento creado correctamente', 'success');
                this.closeModal();
                await this.loadEvents();
                // Update dashboard statistics after event change
                this.refreshDashboardStatistics();
            } else {
                this.showToast(result.message || 'Error saving event', 'error');
            }
        } catch (error) {
            console.error('Error saving event:', error);
            this.showToast('Error saving event', 'error');
        }
    }

    async handleDelete() {
        if (!this.currentEvent || !this.currentEvent.id) return;
        
        const result = await Swal.fire({
            title: 'Delete Event',
            text: 'Are you sure you want to delete this event? This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, Delete',
            cancelButtonText: 'No, Cancel'
        });
        
        if (result.isConfirmed) {
            try {
                const response = await fetch(`/Calendar/DeleteEvent?id=${this.currentEvent.id}`, {
                    method: 'DELETE',
                    headers: {
                        'Content-Type': 'application/json',
                    }
                });
                
                const deleteResult = await response.json();
                
                if (deleteResult.success) {
                    this.showToast('Evento eliminado correctamente', 'success');
                    this.closeModal();
                    await this.loadEvents();
                    // Update dashboard statistics after event deletion
                    this.refreshDashboardStatistics();
                } else {
                    this.showToast(deleteResult.message || 'Error deleting event', 'error');
                }
            } catch (error) {
                console.error('Error deleting event:', error);
                this.showToast('Error deleting event', 'error');
            }
        }
    }

    // Utility methods
    parseDateTime(dateTimeString) {
        // Parse "YYYY-MM-DD HH:MM" format
        const [datePart, timePart] = dateTimeString.split(' ');
        const [year, month, day] = datePart.split('-').map(Number);
        const [hours, minutes] = timePart.split(':').map(Number);
        
        return new Date(year, month - 1, day, hours, minutes);
    }

    formatDateTimeForInput(date) {
        // Format for datetime-local input (YYYY-MM-DDTHH:MM)
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        
        return `${year}-${month}-${day}T${hours}:${minutes}`;
    }

    formatDateTimeForServer(date) {
        // Format for server (YYYY-MM-DD HH:MM)
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        
        return `${year}-${month}-${day} ${hours}:${minutes}`;
    }

    showLoading() {
        document.getElementById('loading').classList.remove('hidden');
        document.getElementById('error').classList.add('hidden');
        document.getElementById('calendar').classList.add('hidden');
    }

    showError(message) {
        document.getElementById('loading').classList.add('hidden');
        document.getElementById('error').classList.remove('hidden');
        document.getElementById('calendar').classList.add('hidden');
        document.getElementById('error-message').textContent = message;
    }

    showCalendar() {
        document.getElementById('loading').classList.add('hidden');
        document.getElementById('error').classList.add('hidden');
        document.getElementById('calendar').classList.remove('hidden');
        
        // Force calendar to recalculate its size after showing
        setTimeout(() => {
            if (this.calendar) {
                this.calendar.updateSize();
            }
        }, 100);
    }

    showToast(message, type = 'info') {
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        });

        Toast.fire({
            icon: type,
            title: message
        });
    }

    // Dashboard Statistics Methods
    async updateDashboardStatistics(startDate, endDate) {
        try {
            this.showDashboardLoading();
            
            // Adjust endDate to include the full last day until 23:59:59
            // FullCalendar's endDate is exclusive (next day at 00:00), but we want inclusive (same day at 23:59:59)
            const adjustedEndDate = new Date(endDate);
            adjustedEndDate.setDate(adjustedEndDate.getDate() - 1);
            adjustedEndDate.setHours(23, 59, 59, 999); // Set to end of day
            
            // Debug: Log the dates being sent
            console.log('Dashboard Statistics Request:');
            console.log('Original startDate:', startDate);
            console.log('Original endDate:', endDate);
            console.log('Adjusted endDate (end of day):', adjustedEndDate);
            console.log('Formatted startDate:', this.formatDateForQuery(startDate));
            console.log('Formatted endDate:', this.formatDateForQuery(adjustedEndDate));
            
            const queryParams = new URLSearchParams({
                startDate: this.formatDateForQuery(startDate),
                endDate: this.formatDateForQuery(adjustedEndDate),
                weekStart: this.formatDateForQuery(startDate)
            });

            console.log('Query URL:', `/Calendar/GetDashboardStatistics?${queryParams}`);

            const response = await fetch(`/Calendar/GetDashboardStatistics?${queryParams}`);
            const result = await response.json();

            console.log('Dashboard Statistics Response:', result);

            if (result.success) {
                this.updateDashboardDisplay(result.data);
            } else {
                console.error('Error fetching dashboard statistics:', result.message);
            }
        } catch (error) {
            console.error('Error fetching dashboard statistics:', error);
        } finally {
            this.hideDashboardLoading();
        }
    }

    formatDateForQuery(date) {
        // Check if the date has specific time (not midnight)
        const hasSpecificTime = date.getHours() !== 0 || date.getMinutes() !== 0 || date.getSeconds() !== 0;
        
        if (hasSpecificTime) {
            // Format as YYYY-MM-DD HH:MM:SS for dates with specific time
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            const seconds = String(date.getSeconds()).padStart(2, '0');
            return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
        } else {
            // Format date as YYYY-MM-DD for dates at midnight
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        }
    }

    getDayOfWeek(dateString) {
        if (!dateString) return 'Unknown';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('en-US', { weekday: 'long' });
        } catch (error) {
            console.error('Error getting day of week:', error);
            return 'Unknown';
        }
    }

    showDashboardLoading() {
        const loadingElements = [
            'total-events-loading',
            'busiest-day-events-loading',
            'longest-day-loading',
            'peak-occupancy-loading'
        ];
        
        const valueElements = [
            'total-events-value',
            'busiest-day-events-value',
            'longest-day-value',
            'peak-occupancy-value'
        ];

        loadingElements.forEach(id => {
            const element = document.getElementById(id);
            if (element) element.classList.remove('hidden');
        });

        valueElements.forEach(id => {
            const element = document.getElementById(id);
            if (element) element.classList.add('hidden');
        });
    }

    hideDashboardLoading() {
        const loadingElements = [
            'total-events-loading',
            'busiest-day-events-loading',
            'longest-day-loading',
            'peak-occupancy-loading'
        ];
        
        const valueElements = [
            'total-events-value',
            'busiest-day-events-value',
            'longest-day-value',
            'peak-occupancy-value'
        ];

        loadingElements.forEach(id => {
            const element = document.getElementById(id);
            if (element) element.classList.add('hidden');
        });

        valueElements.forEach(id => {
            const element = document.getElementById(id);
            if (element) element.classList.remove('hidden');
        });
    }

    updateDashboardDisplay(data) {
        // Update total events
        const totalEventsElement = document.getElementById('total-events-value');
        if (totalEventsElement) {
            totalEventsElement.textContent = data.totalEvents || 0;
        }

        // Update busiest day (events)
        const busiestDayElement = document.getElementById('busiest-day-events-value');
        if (busiestDayElement) {
            if (data.dayWithMostEvents && data.dayWithMostEvents.date !== 'Unknown') {
                const dayName = this.getDayOfWeek(data.dayWithMostEvents.date);
                busiestDayElement.textContent = `${dayName} (${data.dayWithMostEvents.eventCount})`;
            } else {
                busiestDayElement.textContent = '-';
            }
        }

        // Update longest day
        const longestDayElement = document.getElementById('longest-day-value');
        if (longestDayElement) {
            if (data.dayWithMostHours && data.dayWithMostHours.date !== 'Unknown') {
                const dayName = this.getDayOfWeek(data.dayWithMostHours.date);
                longestDayElement.textContent = `${dayName} (${data.dayWithMostHours.totalHours.toFixed(1)}h)`;
            } else {
                longestDayElement.textContent = '-';
            }
        }

        // Update peak occupancy
        const peakOccupancyElement = document.getElementById('peak-occupancy-value');
        if (peakOccupancyElement) {
            if (data.peakOccupancy && data.peakOccupancy.date !== 'Unknown') {
                const dayName = this.getDayOfWeek(data.peakOccupancy.date);
                peakOccupancyElement.textContent = `${dayName} (${data.peakOccupancy.occupancyPercentage.toFixed(2)}%)`;
            } else {
                peakOccupancyElement.textContent = '-';
            }
        }
    }

    refreshDashboardStatistics() {
        // Get current calendar view dates and refresh statistics
        if (this.calendar) {
            const view = this.calendar.view;
            this.updateDashboardStatistics(view.activeStart, view.activeEnd);
            this.updateStatisticsTitle(view.type, view.activeStart, view.activeEnd);
        }
    }

    updateStatisticsTitle(viewType, startDate, endDate) {
        const titleElement = document.getElementById('statistics-title');
        const descriptionElement = document.getElementById('total-events-description');
        
        if (!titleElement) return;

        let title = '';
        let description = '';
        
        switch (viewType) {
            case 'dayGridMonth':
                title = `Monthly Statistics (${this.formatMonthYear(startDate)})`;
                description = 'Events in current month';
                break;
            case 'timeGridWeek':
                title = `Weekly Statistics (${this.formatDateRange(startDate, endDate)})`;
                description = 'Events in current week';
                break;
            case 'timeGridDay':
                title = `Daily Statistics (${this.formatSingleDate(startDate)})`;
                description = 'Events on this day';
                break;
            default:
                title = `Statistics (${this.formatDateRange(startDate, endDate)})`;
                description = 'Events in current period';
        }
        
        titleElement.textContent = title;
        if (descriptionElement) {
            descriptionElement.textContent = description;
        }
    }

    formatMonthYear(date) {
        return date.toLocaleDateString('en-US', { 
            month: 'long', 
            year: 'numeric' 
        });
    }

    formatDateRange(startDate, endDate) {
        const start = startDate.toLocaleDateString('en-US', { 
            month: 'short', 
            day: 'numeric' 
        });
        const end = new Date(endDate.getTime() - 1).toLocaleDateString('en-US', { 
            month: 'short', 
            day: 'numeric',
            year: 'numeric'
        });
        return `${start} - ${end}`;
    }

    formatSingleDate(date) {
        return date.toLocaleDateString('en-US', { 
            weekday: 'long',
            month: 'long', 
            day: 'numeric',
            year: 'numeric'
        });
    }
}

// Initialize calendar when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    new CalendarApp();
}); 