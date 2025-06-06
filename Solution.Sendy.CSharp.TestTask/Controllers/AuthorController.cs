﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Sendy.CSharp.TestTask.DataBase;
using Solution.Sendy.CSharp.TestTask.DataBase.Models;
using Solution.Sendy.CSharp.TestTask.DTOs;

namespace Solution.Sendy.CSharp.TestTask.Controllers;

/// <summary>
/// Контроллер для управления авторами книг
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthorController : ControllerBase
{
    // Контекст БД для доступа к ней, маппер для взаимодействия между моделями
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;

    public AuthorController(DatabaseContext db, IMapper mapper)
    {
        _context = db;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить список всех авторов
    /// </summary>
    /// <returns>Список авторов</returns>
    /// <response code="200">Возвращает список авторов</response>
    /// <response code="404">Если авторы не найдены</response>
    /// <response code="401">Если отсутствует API ключ</response>
    /// <response code="403">Если API ключ неверный</response>
    // GET список
    [HttpGet]
    [ProducesResponseType(typeof(List<AuthorDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllAuthorsAsync()
    {
        // Получаем список авторов
        var authors = await _context.Authors.ToListAsync();

        // Если список пустой - 404 код
        if (!authors.Any()) throw new InvalidOperationException("В базе данных отсутствуют авторы");

        // Код 200. Возвращаем DTO-объект клиенту
        return Ok(_mapper.Map<List<AuthorDTO>>(authors));
    }

    /// <summary>
    /// Получить автора по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор автора</param>
    /// <returns>Информация об авторе</returns>
    /// <response code="200">Возвращает информацию об авторе</response>
    /// <response code="400">Если идентификатор некорректный</response>
    /// <response code="404">Если автор не найден</response>
    /// <response code="401">Если отсутствует API ключ</response>
    /// <response code="403">Если API ключ неверный</response>
    // GET по Id
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuthorAsync(int id)
    {
        // Проверяем корректность Id
        if (id <= 0) throw new ArgumentException("Id автора не может быть меньше единицы");

        // Получаем конкретного автора по его Id
        var author = await _context.Authors.FindAsync(id);

        // Если нет такой записи - 404 код
        if (author is null) throw new KeyNotFoundException($"Автор с Id={id} не найден");

        // Код 200. Возвращаем DTO-объект клиенту
        return Ok(_mapper.Map<AuthorDTO>(author));
    }

    /// <summary>
    /// Создать нового автора
    /// </summary>
    /// <param name="dto">Данные для создания автора</param>
    /// <returns>Созданный автор</returns>
    /// <response code="201">Автор успешно создан</response>
    /// <response code="400">Если данные некорректные или автор с таким email уже существует</response>
    /// <response code="401">Если отсутствует API ключ</response>
    /// <response code="403">Если API ключ неверный</response>
    // POST
    [HttpPost]
    [ProducesResponseType(typeof(AuthorDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAuthorAsync([FromBody] CreateAuthorDTO dto)
    {
        // Создаём автора из переданных данных клиента
        var author = _mapper.Map<Author>(dto);

        // Проверяем, нет ли автора с таким же email. Если есть - 400 код
        var existingAuthor = _context.Authors.FirstOrDefault(a => a.Email == dto.Email);
        if (!(existingAuthor is null)) throw new ArgumentException($"Автор с email {dto.Email} уже существует");

        // Добавляем созданного автора и сохраняем изменения в БД
        await _context.Authors.AddAsync(author);
        await _context.SaveChangesAsync();

        // Код 201. Успешное создание записи
        return CreatedAtRoute(new { id = author.AuthorId }, _mapper.Map<AuthorDTO>(author));
    }

    /// <summary>
    /// Обновить информацию об авторе
    /// </summary>
    /// <param name="id">Идентификатор автора</param>
    /// <param name="dto">Обновленные данные автора</param>
    /// <returns>Пустой ответ</returns>
    /// <response code="204">Автор успешно обновлен</response>
    /// <response code="400">Если идентификатор или данные некорректные</response>
    /// <response code="404">Если автор не найден</response>
    /// <response code="401">Если отсутствует API ключ</response>
    /// <response code="403">Если API ключ неверный</response>
    // PUT
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAuthorAsunc(int id, [FromBody] UpdateAuthorDTO dto)
    {
        // Проверяем корректность Id
        if (id <= 0) throw new ArgumentException("Id автора не может меньше единицы");

        // Получаем автора по Id
        var author = await _context.Authors.FindAsync(id);

        // Если нет такой записи - 404 код
        if (author is null) throw new KeyNotFoundException($"Автор с Id={id} не найден");

        // Обновляем только предоставленные поля
        if (!string.IsNullOrEmpty(dto.FirstName))
        {
            author.FirstName = dto.FirstName;
        }

        if (!string.IsNullOrEmpty(dto.LastName))
        {
            author.LastName = dto.LastName;
        }

        if (!string.IsNullOrEmpty(dto.Email))
        {
            // Проверяем, нет ли другого автора с таким email
            var existingAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.Email == dto.Email && a.AuthorId != id);
            if (existingAuthor != null)
            {
                throw new ArgumentException($"Автор с email {dto.Email} уже существует");
            }
            author.Email = dto.Email;
        }

        // Сохранение изменений в БД
        await _context.SaveChangesAsync();

        // Код 204. Пустой ответ
        return NoContent();
    }

    /// <summary>
    /// Удалить автора
    /// </summary>
    /// <param name="id">Идентификатор автора</param>
    /// <returns>Подтверждение удаления</returns>
    /// <response code="200">Автор успешно удален</response>
    /// <response code="400">Если идентификатор некорректный</response>
    /// <response code="404">Если автор не найден</response>
    /// <response code="401">Если отсутствует API ключ</response>
    /// <response code="403">Если API ключ неверный</response>
    // DELETE
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAuthorAsync(int id)
    {
        // Проверяем корректность Id
        if (id <= 0) throw new ArgumentException("Id автора не может быть меньше единицы");

        // Получаем автора по Id
        var author = await _context.Authors.FindAsync(id);

        // Если нет такой записи - 404 код
        if (author is null) throw new KeyNotFoundException($"Автор с Id={id} не найден");

        // Удаляем автора и сохраняем изменения в БД
        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        // Код 200. Успешное удаление
        return Ok();
    }
}