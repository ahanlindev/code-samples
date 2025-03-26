public interface ISaveReadWriter
{
    /// <summary>
    /// Attempts to save the provided data to a file at the provided file path.
    /// Will overwrite the file at this path if it already exists.
    /// </summary>
    /// <param name="filePath">Path to save data relative to the game folder. Should include file name and extension.</param>
    /// <param name="saveData">Data object to serialize and save to a file</param>
    /// <returns>True if saving was successful, otherwise false</returns>
    bool SaveGameDataToFile(string filePath, SaveDataV1 saveData);

    /// <summary>
    /// Attempts to read the file at the provided path and parse it for save data 
    /// </summary>
    /// <param name="filePath">Path of the file to read data from. Should include file name and extension.</param>
    /// <param name="saveData">Data object read from the file, or null if the method failed </param>
    /// <returns>True if the file existed and was read successfully, otherwise false</returns>
    bool TryLoadGameDataFromFile(string filePath, out SaveDataV1 saveData);
}