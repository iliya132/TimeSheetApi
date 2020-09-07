﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeSheetApi.Model.Entities
{
    [Table("Departments")]
    public class Departments
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string shortName { get; set; }
    }
}
