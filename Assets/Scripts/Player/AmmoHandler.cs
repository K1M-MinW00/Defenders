using UnityEngine;
using System.Collections;

public class AmmoHandler : MonoBehaviour
{
    public int maxAmmo = 10;
    public float reloadTime = 2f;

    public int CurrentAmmo { get; private set; }
    public bool IsReloading { get; private set; }

    private void Awake()
    {
        CurrentAmmo = maxAmmo;
    }

    public bool CanShoot()
    {
        return CurrentAmmo > 0 && !IsReloading;
    }

    public void ConsumeAmmo()
    {
        CurrentAmmo--;
    }

    public IEnumerator Reload()
    {
        if (IsReloading)
            yield break;

        IsReloading = true;
        Debug.Log("재장전 시작");

        yield return new WaitForSeconds(reloadTime);

        CurrentAmmo = maxAmmo;
        IsReloading = false;
        Debug.Log("재장전 완료");
    }
}
