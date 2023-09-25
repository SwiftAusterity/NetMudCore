﻿namespace NetMudCore.Models
{
    public class SubmitButtonModel
    {
        public string SubmitName { get; set; }
        public string CancelName { get; set; }
        public bool IncludeCancel { get; set; }
        public bool ModalWindow { get; set; }

        public object ReturnRouteValues { get; set; }
        public string ReturnController { get; set; }
        public string ReturnAction { get; set; }

        public SubmitButtonModel()
        {
            IncludeCancel = true;
            ModalWindow = false;
        }
    }
}