using System;

namespace Robot.V2.Network
{
   public enum Command
   {
      StartRobotCameraStream, // robot side
      StopRobotCameraStream, // robot side
   }

   public class TcpCommand
   {
      /// <summary>
      /// Converts a Command enum value to its string representation
      /// </summary>
      /// <param name="command">Command enum value to convert</param>
      /// <returns>String name of the command</returns>
      public static string GetString(Command command)
      {
         return Enum.GetName(typeof(Command), command);
      }

      /// <summary>
      /// Converts a string to its corresponding Command enum value
      /// </summary>
      /// <param name="s">String representation of the command</param>
      /// <returns>Command enum value</returns>
      public static Command GetCommand(string s)
      {
         return Enum.Parse<Command>(s);
      }
   }
}