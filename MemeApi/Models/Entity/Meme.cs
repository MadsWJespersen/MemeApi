﻿using System.Collections.Generic;

namespace MemeApi.Models.Entity
{
    public class Meme : Votable
    {
        public MemeVisual MemeVisual { get; set; }
        public MemeSound MemeSound { get; set; }
        public List<MemeText> MemeTexts { get; set; }
    }
}
