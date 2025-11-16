using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MagnetronTecnicalTest.Controllers;
using MagnetronTecnicalTest.Dtos;
using MagnetronTecnicalTest.Tests.Helpers;

namespace MagnetronTecnicalTest.Tests.Controllers;

public class PersonasControllerTests : TestBase
{
    private PersonasController _controller;

    public PersonasControllerTests()
    {
        _controller = new PersonasController(Context);
        SeedDatabase();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllPersonas()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var personas = okResult!.Value as IEnumerable<PersonaDto>;
        personas.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_WithValidId_ShouldReturnPersona()
    {
        // Act
        var result = await _controller.Get(1);

        // Assert
        result.Result.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(1);
        result.Value.Nombre.Should().Be("Juan");
    }

    [Fact]
    public async Task Get_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.Get(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldCreatePersona()
    {
        // Arrange
        var dto = new CreatePersonaDto("Carlos", "López", "DNI", "11111111");

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var persona = createdResult!.Value as PersonaDto;
        persona.Should().NotBeNull();
        persona!.Nombre.Should().Be("Carlos");
        persona.Documento.Should().Be("11111111");
    }

    [Fact]
    public async Task Create_WithDuplicateDocumento_ShouldReturnConflict()
    {
        // Arrange
        var dto = new CreatePersonaDto("Test", "User", "DNI", "12345678"); // Documento ya existe

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Update_WithValidData_ShouldUpdatePersona()
    {
        // Arrange
        var dto = new CreatePersonaDto("Juan Carlos", "Pérez Silva", "DNI", "12345678");

        // Act
        var result = await _controller.Update(1, dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify the update
        var updatedPersona = await Context.Personas.FindAsync(1L);
        updatedPersona!.Nombre.Should().Be("Juan Carlos");
        updatedPersona.Apellido.Should().Be("Pérez Silva");
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new CreatePersonaDto("Test", "User", "DNI", "99999999");

        // Act
        var result = await _controller.Update(999, dto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_WithDuplicateDocumento_ShouldReturnConflict()
    {
        // Arrange
        var dto = new CreatePersonaDto("Juan", "Pérez", "DNI", "87654321"); // Documento de otra persona

        // Act
        var result = await _controller.Update(1, dto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeletePersona()
    {
        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify deletion
        var persona = await Context.Personas.FindAsync(1L);
        persona.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}