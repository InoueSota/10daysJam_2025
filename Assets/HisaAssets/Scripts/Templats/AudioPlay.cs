using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay: MonoBehaviour
{
    [SerializeField] private AudioSource a1;//AudioSource�^�̕ϐ�a1��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioSource a2;//AudioSource�^�̕ϐ�a2��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioSource a3;//AudioSource�^�̕ϐ�a3��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioSource a4;//AudioSource�^�̕ϐ�a3��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioSource a5;//AudioSource�^�̕ϐ�a3��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioSource a6;//AudioSource�^�̕ϐ�a3��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioSource a7;//AudioSource�^�̕ϐ�a3��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioSource a8;//AudioSource�^�̕ϐ�a3��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioSource a9;//AudioSource�^�̕ϐ�a3��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v

    [SerializeField] private AudioClip b1;//AudioClip�^�̕ϐ�b1��錾 �g�p����AudioClip���A�^�b�`�K�v
    [SerializeField] private AudioClip b2;//AudioClip�^�̕ϐ�b2��錾 �g�p����AudioClip���A�^�b�`�K�v 
    [SerializeField] private AudioClip b3;//AudioClip�^�̕ϐ�b3��錾 �g�p����AudioClip���A�^�b�`�K�v 
    [SerializeField] private AudioClip b4;//AudioClip�^�̕ϐ�b3��錾 �g�p����AudioClip���A�^�b�`�K�v 
    [SerializeField] private AudioClip b5;//AudioClip�^�̕ϐ�b3��錾 �g�p����AudioClip���A�^�b�`�K�v 
    [SerializeField] private AudioClip b6;//AudioClip�^�̕ϐ�b3��錾 �g�p����AudioClip���A�^�b�`�K�v 
    [SerializeField] private AudioClip b7;//AudioClip�^�̕ϐ�b3��錾 �g�p����AudioClip���A�^�b�`�K�v 
    [SerializeField] private AudioClip b8;//AudioClip�^�̕ϐ�b3��錾 �g�p����AudioClip���A�^�b�`�K�v 
    [SerializeField] private AudioClip b9;//AudioClip�^�̕ϐ�b3��錾 �g�p����AudioClip���A�^�b�`�K�v 

    //����̊֐�1
    public void SE1()
    {
        a1.PlayOneShot(b1);//a1�ɃA�^�b�`����AudioSource�̐ݒ�l��b1�ɃA�^�b�`�������ʉ����Đ�
    }

    //����̊֐�2
    public void SE2()
    {
        a2.PlayOneShot(b2);//a2�ɃA�^�b�`����AudioSource�̐ݒ�l��b2�ɃA�^�b�`�������ʉ����Đ�
    }

    //����̊֐�3
    public void SE3()
    {
        a3.PlayOneShot(b3);//a3�ɃA�^�b�`����AudioSource�̐ݒ�l��b3�ɃA�^�b�`�������ʉ����Đ�
    }

    //����̊֐�3
    public void SE4()
    {
        a4.PlayOneShot(b4);//a3�ɃA�^�b�`����AudioSource�̐ݒ�l��b3�ɃA�^�b�`�������ʉ����Đ�
    }

    //����̊֐�3
    public void SE5()
    {
        a5.PlayOneShot(b5);//a3�ɃA�^�b�`����AudioSource�̐ݒ�l��b3�ɃA�^�b�`�������ʉ����Đ�
    }

    //����̊֐�3
    public void SE6()
    {
        a6.PlayOneShot(b6);//a3�ɃA�^�b�`����AudioSource�̐ݒ�l��b3�ɃA�^�b�`�������ʉ����Đ�
    }

    //����̊֐�3
    public void SE7()
    {
        a7.PlayOneShot(b7);//a3�ɃA�^�b�`����AudioSource�̐ݒ�l��b3�ɃA�^�b�`�������ʉ����Đ�
    }

    //����̊֐�3
    public void SE8()
    {
        a8.PlayOneShot(b8);//a3�ɃA�^�b�`����AudioSource�̐ݒ�l��b3�ɃA�^�b�`�������ʉ����Đ�
    }

    //����̊֐�3
    public void SE9()
    {
        a9.PlayOneShot(b9);//a3�ɃA�^�b�`����AudioSource�̐ݒ�l��b3�ɃA�^�b�`�������ʉ����Đ�
    }
}
