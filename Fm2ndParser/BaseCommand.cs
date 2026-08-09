using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fm2ndParser
{
    public class BaseCommand
    {
        protected string generateDefaultOutputFolder()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        protected void validateInputs(string output)
        {
            if (Directory.Exists(output))
                throw new Exception($"Output folder '{output}' already exists. Please specify a different output folder.");
        }
    }
}
