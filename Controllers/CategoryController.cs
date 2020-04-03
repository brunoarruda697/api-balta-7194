using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Category>>> Get(
            [FromServices]DataContext context
        )
        {
            //AsNoTracking faz uma requisição mais rápida pois não traz alguns dados como versionamento por exemplo
            //Filtros devem ser feitos antes de executar o ToListAsync pra não dar gargalo de memória
            var categories = await context.Categories.AsNoTracking().ToListAsync();
            return Ok(categories);
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Category>> GetById(
            int id,
            [FromServices]DataContext context
        )
        {
            var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return Ok(category);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Post(
            [FromBody]Category model,
            [FromServices]DataContext context
        )
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Categories.Add(model);
                //SaveChangesAsync cria um id pro model e já popula ele no context
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar a categoria"});
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Put(
            int id,
            [FromBody]Category model,
            [FromServices]DataContext context
        )
        {
            if(id != model.Id)
                return NotFound(new { message = "Categoria não encontrada" });
            
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<Category>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Este registro já foi atualizada"});
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível autalizar a categoria"});
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Delete(
            int id,
            [FromServices]DataContext context
        )
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if(category == null)
                return NotFound(new { message = "Categoria não encontrada"});

            try
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(category);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possivel deletar a categoria"});
            }
        }
    }
}