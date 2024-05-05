using BookShoppingCartMvcUI.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShoppingCartMvcUI.Controllers;

[Authorize(Roles =nameof(Roles.Admin))]
public class AdminOperationsController : Controller
{
    private readonly IUserOrderRepository _userOrderRepository;

    public AdminOperationsController(IUserOrderRepository userOrderRepository)
    {
            _userOrderRepository = userOrderRepository;
    }
   public async  Task<IActionResult> AllOrders()
    {
        var orders = await _userOrderRepository.UserOrders(true);
        return View(orders);
    }

    public async Task<IActionResult> TogglePaymentStatus(int orderId)
    {
        try
        {
            await _userOrderRepository.TogglePaymentStatus(orderId);
        }
        catch (Exception ex)
        {

           
        }
        return RedirectToAction(nameof(AllOrders));
    }

    public async Task<IActionResult> UpdatePaymentStatus(int orderId)
    {
        var order = await _userOrderRepository.GetOrderById(orderId);

        if (order == null)
        {
            throw new InvalidOperationException($"Order with id:{orderId} does not found.");
        }

        var orderStatusList = (await _userOrderRepository.GetOrderStatuses()).Select(OrderStatus=>
        {
            return new SelectListItem
            {
                Value = OrderStatus.Id.ToString(),
                Text = OrderStatus.StatusName,
                Selected = order.OrderStatusId == OrderStatus.Id
            };
        });

        var data = new UpdateOrderStatusModel
        {
            OrderId = orderId,
            OrderStatusId = order.OrderStatusId,
            OrderStatusList = orderStatusList
        };

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePaymentStatus(UpdateOrderStatusModel data)

    {
        try
        {
            if (!ModelState.IsValid)
            {
                data.OrderStatusList = (await _userOrderRepository.GetOrderStatuses()).Select(orderstatus =>
                {
                    return new SelectListItem
                    {
                        Value = orderstatus.Id.ToString(),
                        Text = orderstatus.StatusName,
                        Selected = orderstatus.Id == data.OrderStatusId
                    };
                });
                return View(data);
            }
            await _userOrderRepository.ChangeOrderStatus(data);
            TempData["msg"] = "Updated successfully";
        }
        catch (Exception ex)
        {
            TempData["msg"] = "Something went wrong";
        }

        return RedirectToAction(nameof(UpdatePaymentStatus), new { orderId = data.OrderId });
    }
    


}

