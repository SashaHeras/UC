﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLEdition.DAL.ViewModels
{
    public class UserAnswersModel
    {
        public int TaskId { get; set; }

        public int AnswerId { get; set; }

        public bool IsChecked { get; set; }
    }
}
