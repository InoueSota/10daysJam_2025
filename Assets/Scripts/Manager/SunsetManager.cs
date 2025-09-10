using UnityEngine;
using static UnityEngine.ParticleSystem;

public class SunsetManager : MonoBehaviour
{
    [SerializeField] private bool isSunsetActive;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] ParticleInstantiateScript particle;

    private SoundInstantiateScript sound;

    private void Start()
    {
        particle = GetComponent<ParticleInstantiateScript>();
        sound = GetComponent<SoundInstantiateScript>();
    }
    public void StartDestroyRay(Vector3 _rocketVector, Vector3 _playerPosition, bool _isDivision, Vector3 _divisionLinePosition, int _divisionMode)
    {
        // 境界線が引かれている
        if (_isDivision)
        {
            // _divisionMode == 0 == HORIZONTAL
            if (_divisionMode == 0)
            {
                // 境界線よりも上にいる
                if (_playerPosition.y > _divisionLinePosition.y)
                {
                    // 左移動をしていた
                    if (_rocketVector.x < -0f)
                    {
                        // 左上座標
                        Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)) + Vector3.right * 0.5f;
                        // 左下座標
                        Vector3 rayPosition2 = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x, _divisionLinePosition.y + 0.5f, 0f)  + Vector3.right * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.right, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height * 0.5f, 0f)));
                    }
                    // 右移動をしていた
                    else if (_rocketVector.x > 0f)
                    {
                        // 右上座標
                        Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, Screen.height, 0f)) + Vector3.left * 0.5f;
                        // 右下座標
                        Vector3 rayPosition2 = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)).x, _divisionLinePosition.y + 0.5f, 0f) + Vector3.left * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.left, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height * 0.5f, 0f)));
                    }
                    // 上移動をしていた
                    else if (_rocketVector.y > 0f)
                    {
                        // 左上座標
                        Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)) + Vector3.down * 0.5f;
                        // 右上座標
                        Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, Screen.height, 0f)) + Vector3.down * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.down, _divisionLinePosition);
                    }
                    // 下移動をしていた
                    else if (_rocketVector.y < -0f)
                    {
                        // 左下座標
                        Vector3 rayPosition1 = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x, _divisionLinePosition.y + 0.5f, 0f) + Vector3.up * 0.5f;
                        // 右下座標
                        Vector3 rayPosition2 = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)).x, _divisionLinePosition.y + 0.5f, 0f) + Vector3.up * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.up, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)));
                    }
                }
                // 境界線よりも下にいる
                else
                {
                    // 左移動をしていた
                    if (_rocketVector.x < -0f)
                    {
                        // 左上座標
                        Vector3 rayPosition1 = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x, _divisionLinePosition.y - 0.5f, 0f) + Vector3.right * 0.5f;
                        // 左下座標
                        Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)) + Vector3.right * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.right, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height * 0.5f, 0f)));
                    }
                    // 右移動をしていた
                    else if (_rocketVector.x > 0f)
                    {
                        // 右上座標
                        Vector3 rayPosition1 = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)).x, _divisionLinePosition.y - 0.5f, 0f) + Vector3.left * 0.5f;
                        // 右下座標
                        Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)) + Vector3.left * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.left, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height * 0.5f, 0f)));
                    }
                    // 上移動をしていた
                    else if (_rocketVector.y > 0f)
                    {
                        // 左上座標
                        Vector3 rayPosition1 = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x, _divisionLinePosition.y - 0.5f, 0f) + Vector3.down * 0.5f;
                        // 右上座標
                        Vector3 rayPosition2 = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)).x, _divisionLinePosition.y - 0.5f, 0f) + Vector3.down * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.down, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)));
                    }
                    // 下移動をしていた
                    else if (_rocketVector.y < -0f)
                    {
                        // 左下座標
                        Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)) + Vector3.up * 0.5f;
                        // 右下座標
                        Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)) + Vector3.up * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.up, _divisionLinePosition);
                    }
                }
            }
            // _divisionMode == 1 == VERTICAL
            else if (_divisionMode == 1)
            {
                // 境界線よりも左にいる
                if (_playerPosition.x < _divisionLinePosition.x)
                {
                    // 左移動をしていた
                    if (_rocketVector.x < -0f)
                    {
                        // 左上座標
                        Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)) + Vector3.right * 0.5f;
                        // 左下座標
                        Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)) + Vector3.right * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.right, _divisionLinePosition);
                    }
                    // 右移動をしていた
                    else if (_rocketVector.x > 0f)
                    {
                        // 右上座標
                        Vector3 rayPosition1 = new Vector3(_divisionLinePosition.x - 0.5f, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y, 0f) + Vector3.left * 0.5f;
                        // 右下座標
                        Vector3 rayPosition2 = new Vector3(_divisionLinePosition.x - 0.5f, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y, 0f) + Vector3.left * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.left, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height * 0.5f, 0f)));
                    }
                    // 上移動をしていた
                    else if (_rocketVector.y > 0f)
                    {
                        // 左上座標
                        Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)) + Vector3.down * 0.5f;
                        // 右上座標
                        Vector3 rayPosition2 = new Vector3(_divisionLinePosition.x - 0.5f, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y, 0f) + Vector3.down * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.down, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)));
                    }
                    // 下移動をしていた
                    else if (_rocketVector.y < -0f)
                    {
                        // 左下座標
                        Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)) + Vector3.up * 0.5f;
                        // 右下座標
                        Vector3 rayPosition2 = new Vector3(_divisionLinePosition.x - 0.5f, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y, 0f) + Vector3.up * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.up, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)));
                    }
                }
                // 境界線よりも右にいる
                else
                {
                    // 左移動をしていた
                    if (_rocketVector.x < -0f)
                    {
                        // 左上座標
                        Vector3 rayPosition1 = new Vector3(_divisionLinePosition.x + 0.5f, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y, 0f) + Vector3.right * 0.5f;
                        // 左下座標
                        Vector3 rayPosition2 = new Vector3(_divisionLinePosition.x + 0.5f, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y, 0f) + Vector3.right * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.right, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, Screen.height * 0.5f, 0f)));
                    }
                    // 右移動をしていた
                    else if (_rocketVector.x > 0f)
                    {
                        // 右上座標
                        Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, Screen.height, 0f)) + Vector3.left * 0.5f;
                        // 右下座標
                        Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)) + Vector3.left * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.left, _divisionLinePosition);
                    }
                    // 上移動をしていた
                    else if (_rocketVector.y > 0f)
                    {
                        // 左上座標
                        Vector3 rayPosition1 = new Vector3(_divisionLinePosition.x + 0.5f, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y, 0f) + Vector3.down * 0.5f;
                        // 右上座標
                        Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, Screen.height, 0f)) + Vector3.down * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.down, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)));
                    }
                    // 下移動をしていた
                    else if (_rocketVector.y < -0f)
                    {
                        // 左下座標
                        Vector3 rayPosition1 = new Vector3(_divisionLinePosition.x + 0.5f, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y, 0f) + Vector3.up * 0.5f;
                        // 右下座標
                        Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)) + Vector3.up * 0.5f;
                        // 破壊光線の生成
                        CheckDestroy(rayPosition1, rayPosition2, Vector3.up, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)));
                    }
                }
            }
        }
        // 境界線が引かれていない
        else
        {
            // 左移動をしていた
            if (_rocketVector.x < -0f)
            {
                // 左上座標
                Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)) + Vector3.right * 0.5f;
                // 左下座標
                Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)) + Vector3.right * 0.5f;
                // 破壊光線の生成
                CheckDestroy(rayPosition1, rayPosition2, Vector3.right, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height * 0.5f, 0f)));
            }
            // 右移動をしていた
            else if (_rocketVector.x > 0f)
            {
                // 右上座標
                Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, Screen.height, 0f)) + Vector3.left * 0.5f;
                // 右下座標
                Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)) + Vector3.left * 0.5f;
                // 破壊光線の生成
                CheckDestroy(rayPosition1, rayPosition2, Vector3.left, Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height * 0.5f, 0f)));
            }
            // 上移動をしていた
            else if (_rocketVector.y > 0f)
            {
                // 左上座標
                Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)) + Vector3.down * 0.5f;
                // 右上座標
                Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, Screen.height, 0f)) + Vector3.down * 0.5f;
                // 破壊光線の生成
                CheckDestroy(rayPosition1, rayPosition2, Vector3.down, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, 0f, 0f)));
            }
            // 下移動をしていた
            else if (_rocketVector.y < -0f)
            {
                // 左下座標
                Vector3 rayPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)) + Vector3.up * 0.5f;
                // 右下座標
                Vector3 rayPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.75f, 0f, 0f)) + Vector3.up * 0.5f;
                // 破壊光線の生成
                CheckDestroy(rayPosition1, rayPosition2, Vector3.up, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height, 0f)));
            }
        }
    }
    public void CheckDestroy(Vector3 _pointA, Vector3 _pointB, Vector3 _moveDirection, Vector3 _limitPosition)
    {
        // 画面内チェック（0～1の範囲）
        if ((_moveDirection == Vector3.left  && _pointA.x < _limitPosition.x && _pointB.x < _limitPosition.x) ||
            (_moveDirection == Vector3.right && _pointA.x > _limitPosition.x && _pointB.x > _limitPosition.x) ||
            (_moveDirection == Vector3.down  && _pointA.y < _limitPosition.y && _pointB.y < _limitPosition.y) ||
            (_moveDirection == Vector3.up    && _pointA.y > _limitPosition.y && _pointB.y > _limitPosition.y))
        {
            return;
        }

        bool destroyEvenOne = false;

        foreach (RaycastHit2D hit in Physics2D.LinecastAll(_pointA, _pointB, groundLayer))
        {
            // TagがFieldObjectなら
            if (hit && hit.collider.gameObject.CompareTag("FieldObject"))
            {
                particle.RunParticle(0,hit.collider.gameObject.transform.position);
                hit.collider.gameObject.SetActive(false);
                destroyEvenOne = true;
                sound.PlaySound(7, 0.5f);
            }
        }

        if (!destroyEvenOne)
        {
            CheckDestroy(_pointA + _moveDirection, _pointB + _moveDirection, _moveDirection, _limitPosition);
        }
    }

    // Getter
    public bool GetIsSunsetActive() { return isSunsetActive; }
}
