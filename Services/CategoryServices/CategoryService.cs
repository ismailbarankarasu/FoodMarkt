using FoodMart.Dtos.CategoryDtos;
using FoodMart.Entities;
using MongoDB.Driver;

namespace FoodMart.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;

        public CategoryService(IMongoDatabase database)
        {
            _categoryCollection = database.GetCollection<Category>("Categories");
        }

        public async Task<List<ResultCategoryDto>> GetAllAsync()
        {
            var categories = await _categoryCollection
                .Find(_ => true)
                .ToListAsync();

            return categories.Select(x => new ResultCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Icon = x.Icon
            }).ToList();
        }

        public async Task<GetByIdCategoryDto?> GetByIdAsync(string id)
        {
            var category = await _categoryCollection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (category is null)
                return null;

            return new GetByIdCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon
            };
        }

        public async Task CreateAsync(CreateCategoryDto createCategoryDto)
        {
            var category = new Category
            {
                Name = createCategoryDto.Name,
                Icon = createCategoryDto.Icon
            };

            await _categoryCollection.InsertOneAsync(category);
        }

        public async Task UpdateAsync(UpdateCategoryDto updateCategoryDto)
        {
            var category = new Category
            {
                Id = updateCategoryDto.Id,
                Name = updateCategoryDto.Name,
                Icon = updateCategoryDto.Icon
            };

            await _categoryCollection.ReplaceOneAsync(
                x => x.Id == updateCategoryDto.Id,
                category);
        }

        public async Task DeleteAsync(string id)
        {
            await _categoryCollection.DeleteOneAsync(x => x.Id == id);
        }
    }
}