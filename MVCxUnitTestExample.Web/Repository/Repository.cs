using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVCxUnitTestExample.Web.Models;

namespace MVCxUnitTestExample.Web.Repository
{
    public class Repository<T>:IRepository<T> where T:class
    {
        private  MvcXUnitTestDBContext _context { get; }
        private DbSet<T> _dbSet { get;}
        public Repository(MvcXUnitTestDBContext context)
        {
            this._context = context;
            this._dbSet = this._context.Set<T>();
        }
        public async Task Create(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            //_dbSet.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}
