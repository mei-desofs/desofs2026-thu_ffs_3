using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SafeVault.Application.DTOs.Audit;
using SafeVault.Application.DTOs.Auth;
using SafeVault.Application.DTOs.Documents;
using SafeVault.Application.DTOs.Users;
using SafeVault.Application.DTOs.Vaults;
using SafeVault.Application.IServices;
using SafeVault.Domain.Enums;
using SafeVault.InterfaceAdapters;
using SafeVault.InterfaceAdapters.Controllers;

namespace SafeVault.InterfaceAdaptersTests;

public class ControllersCoverageTests
{
    [Fact]
    public async Task AuthController_Login_ShouldReturnOk()
    {
        var authService = new Mock<IAuthService>();
        var expected = new AuthResponse("access", DateTime.UtcNow.AddMinutes(30), "refresh", DateTime.UtcNow.AddDays(7));
        authService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new AuthController(authService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.Request.Headers.UserAgent = "test-agent";

        var response = await controller.Login(new LoginRequest("user@test.com", "Password!123"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task UsersController_GetById_ShouldReturnNotFound_WhenMissing()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserDto?)null);

        var controller = new UsersController(userService.Object);

        var response = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task UsersController_Create_ShouldReturnCreatedAtAction()
    {
        var dto = new UserDto(Guid.NewGuid(), "created@test.com", UserRole.Manager, true, DateTime.UtcNow, null);
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.CreateAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var controller = new UsersController(userService.Object);

        var response = await controller.Create(new CreateUserRequest(dto.Email, "Password!123", dto.Role), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(dto, created.Value);
    }

    [Fact]
    public async Task VaultsController_GetAccessible_ShouldReturnOk()
    {
        var actorId = Guid.NewGuid();
        var vaultService = new Mock<IVaultService>();
        vaultService.Setup(x => x.GetAccessibleAsync(actorId, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<VaultDto>());

        var controller = new VaultsController(vaultService.Object)
        {
            ControllerContext = CreateControllerContext(actorId)
        };

        var response = await controller.GetAccessible(CancellationToken.None);

        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task VaultsController_Archive_ShouldReturnNoContent()
    {
        var actorId = Guid.NewGuid();
        var vaultService = new Mock<IVaultService>();

        var controller = new VaultsController(vaultService.Object)
        {
            ControllerContext = CreateControllerContext(actorId)
        };

        var response = await controller.Archive(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task DocumentsController_Upload_ShouldReturnOk()
    {
        var actorId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var documentDto = new DocumentDto(Guid.NewGuid(), vaultId, "doc.pdf", "application/pdf", 3,
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DocumentClassification.Internal, false, DateTime.UtcNow, 1);

        var documentService = new Mock<IDocumentService>();
        documentService.Setup(x => x.UploadAsync(actorId, It.IsAny<UploadDocumentRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(documentDto);

        var controller = new DocumentsController(documentService.Object)
        {
            ControllerContext = CreateControllerContext(actorId)
        };

        using var stream = new MemoryStream([1, 2, 3]);
        IFormFile formFile = new FormFile(stream, 0, stream.Length, "file", "doc.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var response = await controller.Upload(vaultId, DocumentClassification.Internal, formFile, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(documentDto, ok.Value);
    }

    [Fact]
    public async Task DocumentsController_Download_ShouldReturnFileResult()
    {
        var actorId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        var download = new DownloadDocumentResponse("doc.pdf", "application/pdf", new MemoryStream([1, 2, 3]));
        var documentService = new Mock<IDocumentService>();
        documentService.Setup(x => x.DownloadAsync(actorId, docId, It.IsAny<CancellationToken>())).ReturnsAsync(download);

        var controller = new DocumentsController(documentService.Object)
        {
            ControllerContext = CreateControllerContext(actorId)
        };

        var response = await controller.Download(docId, CancellationToken.None);

        var file = Assert.IsType<FileStreamResult>(response);
        Assert.Equal("application/pdf", file.ContentType);
    }

    [Fact]
    public async Task AuditController_Search_ShouldReturnOk()
    {
        var auditService = new Mock<IAuditService>();
        auditService.Setup(x => x.SearchAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new AuditLogDto(Guid.NewGuid(), AuditEventType.Login, Guid.NewGuid(), null, "User", "127.0.0.1", "agent", DateTime.UtcNow, true, "ok")
            });

        var controller = new AuditController(auditService.Object);

        var response = await controller.Search(null, null, null, CancellationToken.None);

        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public void ResultExtensions_ShouldThrow_WhenClaimIsMissing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Throws<UnauthorizedAccessException>(() => principal.GetRequiredUserId());
    }

    private static ControllerContext CreateControllerContext(Guid userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "test");
        var principal = new ClaimsPrincipal(identity);

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}
