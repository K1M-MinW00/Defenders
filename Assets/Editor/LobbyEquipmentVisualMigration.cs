using UnityEditor;

// Compatibility entry point for the previous Tools menu.
// Modal naming and the shared equipment detail view are maintained by the refactor migration.
public static class LobbyEquipmentVisualMigration
{
    [MenuItem("Tools/UI/Migrate Equipment Slot Visuals")]
    public static void Run() => LobbyUIRefactorMigration.Run();
}
