﻿using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;
using Villa_API.Data;
using Villa_API.Interfaces;
using Villa_API.Models;


namespace Villa_API.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _appDb;
        internal DbSet<T> dbSet;

        public Repository(AppDbContext appDb)
        {
            _appDb = appDb;
            this.dbSet = _appDb.Set<T>();

        }

        public async Task Create(T entity)
        {
            await dbSet.AddAsync(entity);
            await Save();
        }

        public async Task Delete(T entity)
        {
            dbSet.Remove(entity);
            await Save();
        }

        public async Task<T> Get(Expression<Func<T, bool>>? filter = null, bool tracked = true, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            if(filter != null)
            {
                query = query.Where(filter);
            }
			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
			return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int pageSize = 0, int pageNumber = 1)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (pageSize > 0)
            {
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                //skip0.take(5)
                //page number- 2     || page size -5
                //skip(5*(1)) take(5)
                query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }
            if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
			return await query.ToListAsync();
        }

        public async Task Save()
        {
            await _appDb.SaveChangesAsync();
        }


    }
}