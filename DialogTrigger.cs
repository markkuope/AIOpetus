using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject toActivate;

    [SerializeField] private Transform standingPoint;

    private Transform avatar;

    private async void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            avatar = other.transform;

            // disable player input
            avatar.GetComponent<PlayerInput>().enabled = false;

            await Task.Delay(50);

            // teleport the avatar to standing point
            avatar.position = standingPoint.position;
            avatar.rotation = standingPoint.rotation;

            // disable main cam, enable dialog cam
            mainCamera.SetActive(false);
            toActivate.SetActive(true);

            // d?splay cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Recover()
    {
        avatar.GetComponent<PlayerInput>().enabled = true;

        mainCamera.SetActive(true);
        toActivate.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
