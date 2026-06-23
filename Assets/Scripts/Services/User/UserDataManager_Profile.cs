using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task UpdateNicknameAsync(string nickname)
    {
        if (UserData == null)
            return;

        UserData.Profile.Nickname = nickname;

        DocumentReference userRef = firestore.Collection(UsersCollection).Document(CurrentUserId);

        await userRef.UpdateAsync(new Dictionary<string, object> {
            { "Profile.Nickname", nickname }
        });

        RaiseProfileUpdated();
    }

    public async Task UpdateProfileIconAsync(string iconId)
    {
        if (UserData == null)
            return;

        UserData.Profile.IconId = iconId;

        DocumentReference userRef = firestore.Collection(UsersCollection).Document(CurrentUserId);

        await userRef.UpdateAsync(new Dictionary<string, object> {
            { "Profile.IconId", iconId }
        });
        
        RaiseProfileUpdated();
    }
}