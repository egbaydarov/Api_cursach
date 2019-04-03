using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models.Responses
{
    public class NotesDataResponse : Response
    {
        public NotesDataResponse(int status, List<Note> notes) : base(status)
        {
            Notes = notes;
        }
        public List<Note> Notes { get; set; }
    }
}
