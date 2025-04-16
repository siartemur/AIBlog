using AIBlog.Interfaces;
using AIBlog.Models;

public class TagService : ITagService
{
    private readonly IUnitOfWork _unitOfWork;
    public TagService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<Tag>> GetAllTagsAsync() => await _unitOfWork.Tags.GetAllAsync();

    public async Task<Tag?> GetTagByIdAsync(int id) => await _unitOfWork.Tags.GetByIdAsync(id);

    public async Task AddTagAsync(Tag tag)
    {
        await _unitOfWork.Tags.AddAsync(tag);
        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateTagAsync(Tag tag)
    {
        _unitOfWork.Tags.Update(tag);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteTagAsync(int id)
    {
        await _unitOfWork.Tags.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }
}