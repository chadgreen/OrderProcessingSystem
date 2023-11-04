using Azure;
using Azure.Communication.Email;
using BuildingBricks.EventMessages;
using BuildingBricks.Notice.Constants;
using BuildingBricks.Notice.Models;

namespace BuildingBricks.Notice;

public class NoticeServices : ServicesBase
{
	public NoticeServices(ConfigServices configServices) : base(configServices) { }

	public async Task SendOrderConfirmationAsync(OrderPlacedMessage orderPlacedMessage)
	{
		string subject = $"Order Confirmation - {orderPlacedMessage.PurchaseId}";
		string htmlContent = $"<html><body><p>Thank you for your order. Your order number is {orderPlacedMessage.PurchaseId}.</p></body></html>";
		string plainTextContent = $"Thank you for your order. Your order number is {orderPlacedMessage.PurchaseId}.";
		await SendEmailAsync(orderPlacedMessage.CustomerId, NoticeTypes.OrderConfirmation, subject, htmlContent, plainTextContent);
	}

	public async Task SendBackorderNoticeAsync(InventoryReservedMessage inventoryReservedMessage)
	{
		string subject = $"Backorder Notice - {inventoryReservedMessage.OrderId}";
		string htmlContent = $"<html><body><p>Thank you for your order. Unfortunately, the {inventoryReservedMessage.ProductId} - {inventoryReservedMessage.ProductName} is on backorder and will be shipped as soon as it is back in stock.</p></body></html>";
		string plainTextContent = $"Thank you for your order. Your order number is {inventoryReservedMessage.OrderId}. Your order is on backorder.";
		await SendEmailAsync(inventoryReservedMessage.CustomerId, NoticeTypes.BackorderNotice, subject, htmlContent, plainTextContent);
	}

	private async Task SendEmailAsync(
		int customerId,
		int noticeTypeId,
		string subject,
		string htmlContent,
		string plainTextContent)
	{
		using NoticeContext noticeContext = new(_configServices);
		Customer? customer = await noticeContext.Customers.FindAsync(customerId);
		if (customer is not null)
		{
			await noticeContext.NoticeLogs.AddAsync(new()
			{
				NoticeLogId = await SendEmailAsync(subject, htmlContent, plainTextContent, customer.EmailAddress),
				NoticeTypeId = noticeTypeId,
				CustomerId = customer.CustomerId,
				NoticeBody = htmlContent
			});
			await noticeContext.SaveChangesAsync();
		}
	}

	private async Task<string> SendEmailAsync(
		string subject,
		string htmlContent,
		string plainTextContent,
		string recipientAddress)
	{
		EmailClient emailClient = new(_configServices.AzureCommunicationServicesConnectionString);
		EmailSendOperation emailSendOperation = await emailClient.SendAsync(
			WaitUntil.Completed,
			_configServices.NoticeSenderAddress,
			recipientAddress,
			subject,
			htmlContent,
			plainTextContent);
		return emailSendOperation.Id;
	}

}
