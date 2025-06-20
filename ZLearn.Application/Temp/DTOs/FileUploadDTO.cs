using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Temp.DTOs
{
    public class FileUploadDTO
    {
        public FileUploadDTO(string name, string extension, Stream data, string mime, 
            FileType type)
        {
            Name = name;
            Extension = extension;
            Data = data;
            Mime = mime;
            Type = type;
        }

        public string Name { get; private set; }
        public string Extension { get; private set; }
        public Stream Data { get; private set; }
        public string Mime { get; private set; }
        public FileType Type { get; private set; }
    }
}
