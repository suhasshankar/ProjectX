//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProjectX.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AllocateCourse
    {
        public int AId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int TestType { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public Nullable<int> LastUpdateBy { get; set; }
    }
}
