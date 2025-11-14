using ABCRetail.StorageWeb.Services;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ABCRetail.StorageWeb.Pages.Operations;

public sealed class IndexModel : PageModel
{
    private readonly OperationsQueueService _queueService;

    public IndexModel(OperationsQueueService queueService)
    {
        _queueService = queueService;
    }

    public IReadOnlyList<PeekedMessage> Messages { get; private set; } = Array.Empty<PeekedMessage>();

    [BindProperty]
    public string MessageText { get; set; } = string.Empty;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Messages = await _queueService.PeekMessagesAsync(12, cancellationToken);
    }

    public async Task<IActionResult> OnPostEnqueueAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(MessageText))
        {
            ModelState.AddModelError(nameof(MessageText), "Please enter a message before sending it to the queue.");
            await OnGetAsync(cancellationToken);
            return Page();
        }

        await _queueService.EnqueueMessageAsync(MessageText.Trim(), cancellationToken);
        StatusMessage = "Queued a new operations message.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearAsync(CancellationToken cancellationToken)
    {
        await _queueService.ClearAsync(cancellationToken);
        StatusMessage = "Cleared the operations queue.";
        return RedirectToPage();
    }
}
