namespace Ogma3.Data.DTOs
{
    public class FolderMinimalWithParentDto : FolderMinimalDto
    {
        public long? ParentFolderId { get; set; }
        public bool CanAdd { get; set; }
    }
}