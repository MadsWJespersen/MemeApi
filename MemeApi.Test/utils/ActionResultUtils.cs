﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;

namespace MemeApi.Test.utils
{
    internal class ActionResultUtils
    {
        public static async Task<T> ActionResultToValueAndAssertCreated<T>(Task<ActionResult<T>> actionResult) where T : class
        {
            var result = (await actionResult).Result;
            result.Should().NotBeNull();
            result.Should().BeOfType<CreatedAtActionResult>();
            return ((CreatedAtActionResult)result).Value as T;
        }
        public static async Task<T> ActionResultToValueAndAssertOk<T>(Task<ActionResult<T>> actionResult) where T : class
        {
            var result = (await actionResult).Result;
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            return ((OkObjectResult)result).Value as T;
        }

        public static async Task ActionResultAssertNoContent(Task<ActionResult> actionResult)

        {
            var result = (await actionResult);
            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
        }
    }
}
