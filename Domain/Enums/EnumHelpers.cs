using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Domain.Enums
{
    public static class EnumerationExtension
    {
        public static string Description(this Enum value)
        {
            try
            {
                if (value is null) return string.Empty;

                return value.GetType()?
                            .GetMember(value.ToString())?
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName() ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string DescriptionExt(this Enum value)
        {
            try
            {
                if (value is null) return string.Empty;

                return value.GetType()?
                            .GetMember(value.ToString())?
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetDescription() ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static Dictionary<byte, string> GetAsDictionary(Enum enumObject)
        {
            Dictionary<byte, string> enumDictionary = new Dictionary<byte, string>();
            Array enumValuesArray = Enum.GetValues(enumObject.GetType());

            foreach (Object oItem in enumValuesArray)
            {
                FieldInfo fiItem = enumObject.GetType().GetField(oItem.ToString());
                var display = fiItem.GetCustomAttribute<DisplayAttribute>();
                enumDictionary.Add(Convert.ToByte(fiItem.GetRawConstantValue()), display.Name);
            }

            return enumDictionary;
        }

        //public static List<EnumDto> GetEnumDtos(Enum enumObject)
        //{
        //    List<EnumDto> enumDtos = new();
        //    Dictionary<byte, string> dictionaryEnum = EnumerationExtension.GetAsDictionary(enumObject);

        //    foreach (var item in dictionaryEnum)
        //    {
        //        enumDtos.Add(new() { Id = item.Key, Description = item.Value });
        //    }

        //    return enumDtos;
        //}

        //public static List<EnumDto> GetEnumDtosId(Enum enumObject)
        //{
        //    List<EnumDto> enumDtos = new();
        //    Dictionary<byte, string> dictionaryEnum = EnumerationExtension.GetAsDictionary(enumObject);

        //    foreach (var item in dictionaryEnum)
        //    {
        //        enumDtos.Add(new() { Id = item.Key, Description = item.Key.ToString() });
        //    }

        //    return enumDtos;
        //}

        //public static List<EnumDto> ToEnumDtos<TEnum>(this List<TEnum> enumObjects) where TEnum : Enum
        //{
        //    List<EnumDto> enumDtos = new();

        //    foreach (Enum? enumObject in enumObjects)
        //    {
        //        enumDtos.Add(new EnumDto() { Id = Convert.ToInt32(enumObject), Description = enumObject.Description() });
        //    }

        //    return enumDtos;
        //}
    }
}
