﻿namespace MemeApi.library.Services;

using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using MemeApi.library.Extensions;

public class MemeOfTheDayService : IMemeOfTheDayService
{
    private readonly MemeRepository _memeRepository;
    private readonly VisualRepository _visualRepository;
    private readonly TextRepository _textRepository;
    private readonly TopicRepository _topicRepository;
    private readonly IConfiguration _configuration;
    private readonly IMemeRenderingService _memeRenderingService;
    private readonly IMailSender _mailSender;

    public MemeOfTheDayService(MemeRepository memeRepository, VisualRepository visualRepository, TextRepository textRepository, TopicRepository topicRepository, IConfiguration configuration, IMemeRenderingService memeRenderingService, IMailSender mailSender)
    {
        _memeRepository = memeRepository;
        _visualRepository = visualRepository;
        _textRepository = textRepository;
        _topicRepository = topicRepository;
        _configuration = configuration;
        _memeRenderingService = memeRenderingService;
        _mailSender = mailSender;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var currentTime = DateTime.UtcNow;
        if (_memeRepository.HasMemeOfTheDay(currentTime)) return;

        var visual = await _visualRepository.GetRandomVisual();
        var toptext = await _textRepository.GetRandomText();
        var bottomtext = await _textRepository.GetRandomText();
        var topic = await _topicRepository.GetTopicByName("MemeOfTheDay");

        var meme = await _memeRepository.UpsertByComponents(visual, toptext, bottomtext, topic);
        var wekhookUrl = _configuration["MemeOfTheDay.WebHookURL"];

        using (HttpClient httpClient = new HttpClient())
        {
            var message = new Random().Next(10) != 1 ? "Meme Of the Day" : messages.RandomItem();
            var json_payload = new StringContent(
                    "{" +
                        "\"content\":\"" + message + "\"," +
                        "\"username\":\"Hjerneskade(Meme Of The Day)\"," +
                        "\"avatar_url\":\"https://media.mads.monster/default.jpg\"" +
                    "}" ,
                Encoding.UTF8, "application/json");



            MultipartFormDataContent form = new MultipartFormDataContent();
            var imageContent = _memeRenderingService.RenderMeme(meme);
            form.Add(new ByteArrayContent(imageContent, 0, imageContent.Length), "image/png", "shit.png");
            form.Add(json_payload, "payload_json");
            await httpClient.PostAsync(wekhookUrl, form);
            httpClient.Dispose();
        }

        //TODO: add subscribers
        //_mailSender.sendMemeOfTheDayMail(recipient, _memeRenderingService.RenderMeme(meme));
    }

    // out generated text messages
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
