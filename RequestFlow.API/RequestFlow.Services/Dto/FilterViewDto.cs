using System;

namespace RequestFlow.Services.Dto
{
    public class FilterViewDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
