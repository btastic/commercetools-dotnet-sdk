﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using commercetools.Common;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace commercetools.Categories
{
    /// <summary>
    /// Provides access to the functions in the Categories section of the API.
    /// </summary>
    /// <see href="http://dev.commercetools.com/http-api-projects-categories.html"/>
    public class CategoryManager
    {
        #region Constants

        private const string ENDPOINT_PREFIX = "/categories";

        #endregion

        #region Member Variables

        private Client _client;

        #endregion 

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public CategoryManager(Client client)
        {
            _client = client;
        }

        #endregion

        #region API Methods

        /// <summary>
        /// Gets a category by its ID.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Category</returns>
        /// <see href="http://dev.commercetools.com/http-api-projects-categories.html#get-category-by-id"/>
        public async Task<Category> GetCategoryByIdAsync(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                throw new ArgumentException("categoryId is required");
            }

            string endpoint = string.Concat(ENDPOINT_PREFIX, "/", categoryId);
            dynamic response = await _client.GetAsync(endpoint);

            return new Category(response);
        }

        /// <summary>
        /// Queries categories.
        /// </summary>
        /// <param name="where">Where</param>
        /// <param name="sort">Sort</param>
        /// <param name="expansion">Expansion (not yet implemented)</param>
        /// <param name="limit">Limit</param>
        /// <param name="offset">Offset</param>
        /// <returns>CategoryQueryResult</returns>
        /// <see href="http://dev.commercetools.com/http-api-projects-categories.html#query-categories"/>
        public async Task<CategoryQueryResult> QueryCategoriesAsync(string where = null, string sort = null, int limit = -1, int offset = -1)
        {
            NameValueCollection values = new NameValueCollection();

            if (!string.IsNullOrWhiteSpace(where))
            {
                values.Add("where", where);
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                values.Add("sort", sort);
            }

            if (limit > 0)
            {
                values.Add("limit", limit.ToString());
            }

            if (offset >= 0)
            {
                values.Add("offset", offset.ToString());
            }

            dynamic response = await _client.GetAsync(ENDPOINT_PREFIX, values);

            return new CategoryQueryResult(response);
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryDraft">CategoryDraft object</param>
        /// <returns>Category</returns>
        /// <see href="http://dev.commercetools.com/http-api-projects-categories.html#create-a-category"/>
        public async Task<Category> CreateCategoryAsync(CategoryDraft categoryDraft)
        {
            if (categoryDraft == null)
            {
                throw new ArgumentException("categoryDraft cannot be null");
            }

            if (categoryDraft.Name.IsEmpty())
            {
                throw new ArgumentException("Category name is required");
            }

            if (categoryDraft.Slug.IsEmpty())
            {
                throw new ArgumentException("Category slug is required");
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            string payload = JsonConvert.SerializeObject(categoryDraft, settings);
            dynamic response = await _client.PostAsync(ENDPOINT_PREFIX, payload);

            return new Category(response);
        }

        /// <summary>
        /// Updates a category.
        /// </summary>
        /// <param name="category">Category</param>
        /// the changes should be applied.</param>
        /// <param name="actions">The list of update actions to be performed on
        /// the category.</param>
        /// <returns>Category</returns>
        /// <see href="http://dev.commercetools.com/http-api-projects-categories.html#update-category"/>
        public async Task<Category> UpdateCategoryAsync(Category category, List<JObject> actions)
        {
            return await UpdateCategoryAsync(category.Id, category.Version, actions);
        }

        /// <summary>
        /// Updates a category.
        /// </summary>
        /// <param name="categoryId">ID of the category</param>
        /// <param name="version">The expected version of the category on which
        /// the changes should be applied.</param>
        /// <param name="actions">The list of update actions to be performed on
        /// the category.</param>
        /// <returns>Category</returns>
        /// <see href="http://dev.commercetools.com/http-api-projects-categories.html#update-category"/>
        public async Task<Category> UpdateCategoryAsync(string categoryId, int version, List<JObject> actions)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                throw new ArgumentException("Category ID is required");
            }

            if (version < 1)
            {
                throw new ArgumentException("Version is required");
            }

            if (actions == null || actions.Count < 1)
            {
                throw new ArgumentException("One or more update actions is required");
            }

            JObject data = JObject.FromObject(new
            {
                version = version,
                actions = new JArray(actions.ToArray())
            });

            string endpoint = string.Concat(ENDPOINT_PREFIX, "/", categoryId);
            dynamic response = await _client.PostAsync(endpoint, data.ToString());

            return new Category(response);
        }

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>Category</returns>
        /// <see href="http://dev.commercetools.com/http-api-projects-categories.html#delete-category"/>
        public async Task<Category> DeleteCategoryAsync(Category category)
        {
            return await DeleteCategoryAsync(category.Id, category.Version);
        }

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="categoryId">Caregory ID</param>
        /// <param name="version">Caregory version</param>
        /// <returns>Category</returns>
        /// <see href="http://dev.commercetools.com/http-api-projects-categories.html#delete-category"/>
        public async Task<Category> DeleteCategoryAsync(string categoryId, int version)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                throw new ArgumentException("Category ID is required");
            }

            if (version < 1)
            {
                throw new ArgumentException("Version is required");
            }

            var values = new NameValueCollection
            {
                { "version", version.ToString() }
            };

            string endpoint = string.Concat(ENDPOINT_PREFIX, "/", categoryId);
            dynamic response = await _client.DeleteAsync(endpoint, values);

            return new Category(response);
        }

        #endregion
    }
}