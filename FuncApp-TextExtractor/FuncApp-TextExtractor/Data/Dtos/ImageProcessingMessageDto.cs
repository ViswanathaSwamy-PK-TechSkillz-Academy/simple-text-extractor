﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncApp_TextExtractor.Data.Dtos;

public class ImageProcessingMessageDto
{
    public string ImageName { get; set; } = string.Empty;

    public string Language { get; set; } = "en";
}
