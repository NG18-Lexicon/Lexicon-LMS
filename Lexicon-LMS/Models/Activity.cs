﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Lexicon_LMS.Models
{
    public class Activity
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Deadline { get; set; }

        //[Required]
        [Display(Name = "Module ID")]
        public int? ModuleID { get; set; }
        public virtual Module Module { get; set; }

        public virtual ICollection<Document> Documents { get; set; }
    }

    public class Assignment : Activity
    {

    }
}
