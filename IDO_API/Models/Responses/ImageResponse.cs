using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models.Responses
{
    public class ImageResponse : Response
    {
        public ImageResponse(int status, byte[] buffer) : base(status)
        {
            image = buffer;
        }
        public byte[] image { get; set; }
    }
}
