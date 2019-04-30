using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Chapters;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Providers.Chapters
{
    public class ChapterManager : IChapterManager
    {
        private readonly IItemRepository _itemRepo;

        public ChapterManager(IItemRepository itemRepo)
        {
            _itemRepo = itemRepo;
        }

        public void SaveChapters(Guid itemId, List<ChapterInfo> chapters)
            => _itemRepo.SaveChapters(itemId, chapters);
    }
}
