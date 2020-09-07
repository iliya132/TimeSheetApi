﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeSheetApi.Model.Entities
{
    [Table("Block")]
    public class Block
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("blockName")]
        public string BlockName { get; set; }
    }
}
