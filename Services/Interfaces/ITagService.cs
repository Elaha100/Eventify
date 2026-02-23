using Eventify.Models;

namespace Eventify.Services.Interfaces
{
	public interface ITagService
	{
		List<Tag> GetAll();
		Tag? GetById(int tagId);
		Tag Create(Tag tag);
		bool AddTagToEvent(int eventId, int tagId);
		bool RemoveTagFromEvent(int eventId, int tagId);
		List<EventTag> GetTagsForEvent(int eventId);
	}
}
