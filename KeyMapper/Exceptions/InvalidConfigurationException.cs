using System;
using System.Collections.Generic;
using System.Text;

namespace KeyMapper.Exceptions
{
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException() : base()
        {
        }

        public InvalidConfigurationException(string filePath, int line, Exception? innerException = null)
            : base($"\"{filePath}\" is invalid: line {line}", innerException)
        {

        }

        public InvalidConfigurationException(string message) : base(message)
        {
        }

        public InvalidConfigurationException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
