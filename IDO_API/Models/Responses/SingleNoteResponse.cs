using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models.Responses
{
    public class SingleNoteResponse : Response
    {
        public SingleNoteResponse(int status, Note note) : base(status)
        {
            Note = note;
        }
        public Note Note { get; set; }
    }
}
