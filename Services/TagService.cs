using Eventify.Data;
using Eventify.Models;
using Eventify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eventify.Services
{
	public class TagService : ITagService
	{
		private readonly AppDbContext _context;

		public TagService(AppDbContext context)
		{
			_context = context;
		}

		public List<Tag> GetAll()
		{
			return _context.Tags
				.AsNoTracking()
				.OrderBy(t => t.Name)
				.ToList();
		}

		public Tag? GetById(int tagId)
		{
			return _context.Tags
				.AsNoTracking()
				.FirstOrDefault(t => t.TagId == tagId);
		}

		public Tag Create(Tag tag)
		{
			_context.Tags.Add(tag);
			_context.SaveChanges();
			return tag;
		}

		public bool AddTagToEvent(int eventId, int tagId)
		{
			// Kolla om kopplingen redan finns
			var exists = _context.EventTags
				.Any(et => et.EventId == eventId && et.TagId == tagId);

			if (exists) return false;

			var eventTag = new EventTag
			{
				EventId = eventId,
				TagId = tagId
			};

			_context.EventTags.Add(eventTag);
			_context.SaveChanges();
			return true;
		}

		public bool RemoveTagFromEvent(int eventId, int tagId)
		{
			var eventTag = _context.EventTags
				.FirstOrDefault(et => et.EventId == eventId && et.TagId == tagId);

			if (eventTag is null) return false;

			_context.EventTags.Remove(eventTag);
			_context.SaveChanges();
			return true;
		}

		public List<EventTag> GetTagsForEvent(int eventId)
		{
			return _context.EventTags
				.AsNoTracking()
				.Include(et => et.Tag)
				.Where(et => et.EventId == eventId)
				.ToList();
		}
	}
}
