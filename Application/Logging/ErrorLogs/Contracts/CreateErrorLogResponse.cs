using Application.Common.Models;
using Application.Logging.ErrorLogs.Dtos;
using Domain.Enums;

namespace Application.Logging.ErrorLogs.Contracts;

public class CreateErrorLogResponse : Response<ErrorLogDto>
{
}