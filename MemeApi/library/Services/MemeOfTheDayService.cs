namespace MemeApi.library.Services;

using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Entity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class MemeOfTheDayService : IMemeOfTheDayService
{
    private readonly MemeRepository _memeRepository;
    private readonly IConfiguration _configuration;
    private readonly IMemeRenderingService _memeRenderingService;
    private readonly IMailSender _mailSender;

    public MemeOfTheDayService(MemeRepository memeRepository, IConfiguration configuration, IMemeRenderingService memeRenderingService, IMailSender mailSender)
    {
        _memeRepository = memeRepository;
        _configuration = configuration;
        _memeRenderingService = memeRenderingService;
        _mailSender = mailSender;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Meme meme = await _memeRepository.RandomMemeByComponents();
        var wekhookUrl = _configuration["MemeOfTheDay.WebHookURL"];

        using (HttpClient httpClient = new HttpClient())
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            try
            {

                var imageContent = _memeRenderingService.RenderMeme(meme);
                var message = new Random().Next(10) != 1 ? "Meme Of the Day" : messages.RandomItem();
                var json_payload = CreateJsonPayload(message);

                form.Add(new ByteArrayContent(imageContent, 0, imageContent.Length), "image/png", "shit.png");
                form.Add(json_payload, "payload_json");
                await httpClient.PostAsync(wekhookUrl, form);
                httpClient.Dispose();
            }
            catch (Exception _)
            {
                var jsonResponse = JsonConvert.SerializeObject(meme.ToMemeDTO());

                Console.Error.WriteLine(Regex.Replace(jsonResponse, @"[^\x20-\x7E]", "X"));
                Console.WriteLine(Regex.Replace(jsonResponse, @"[^\x20-\x7E]", "X"));
                Console.WriteLine("Failed meme");
                Console.Error.WriteLine("Failed meme");
            }

        }

        //TODO: add subscribers
        //_mailSender.sendMemeOfTheDayMail(recipient, _memeRenderingService.RenderMeme(meme));
    }

    private StringContent CreateJsonPayload(string message)
    {
        return new StringContent(
        "{" +
            "\"content\":\"" + message + "\"," +
            "\"username\":\"Hjerneskade(Meme Of The Day)\"," +
            "\"avatar_url\":\"https://media.mads.monster/default.jpg\"" +
        "}",
        Encoding.UTF8, "application/json");
    }

    // auto generated text messages
    private static readonly List<string> messages = new()
    {
            "Yo this one is fire 🔥🔥🔥",
            "SHEEEESH i am laughing at this one 😆😆😆😆",
            "This one is making me LOL, LMAO even",
            "This shit so cray cray",
            "If you readed this u stupid",
            "If this u read u dum",
            "Literally dummere end ost",
            "Overvej lige halvdelen... af den her EPISKE MEME",
            "Demens er den ultimative blue pill",
            "Jeg edger ikke aktivt, jeg er smart. Det handler om ikke at gå og anticipate det og keep it cool",
            "Find someone that... looks at you the way Hjalte looks at Simon's coq",
            "\"mads, jeg er en coomer connoisseur\" -Hjalte",
            "\"Jeg hygger mig ikke. Jeg sidder bare og venter på jeg dør\" -Bunu",
            "\"Løsningen er at sætte ild til hele Afrika\" -Jakob",
            "\"Jeg går ind for dårlig videnskab Hjalte, du fik mig.\" - Simon",
            "\"Mads i mikroovnen er good shit\" -Hjalte klokken 2 om natten",
            "\"Den er lidt tiltet... ligesom anders\" -Nico",
            "Did you know? Snask undertale 🥶🥶🥶",
            "UNU BUNU!!!!11!11! HVOR ER DU HENNE????",
            "Er der nogen der læser dette?",
            "En dubloon til den første der liker denne meme",
            "Discombobulate",
            "Bengegigt ququmbabakt",
            "lille pumpe",
            "TEXTURLØS ROTTE??????",
            "Nicolai approves of this image",
            "Hjalte disapproves of this image",
            "Bunu coomer over dette billede",
            "Toni... don't hurry up with that 😅",
            "Skaftet",
        };
}

