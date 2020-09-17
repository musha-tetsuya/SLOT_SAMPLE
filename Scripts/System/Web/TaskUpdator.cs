using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 託されたタスクを処理後、自動で消滅するオブジェクト
/// </summary>
public class TaskUpdator : MonoBehaviour
{
    /// <summary>
    /// タスク
    /// </summary>
    private Func<Task> function = null;

    /// <summary>
    /// タスク起動
    /// </summary>
    public static TaskUpdator Run(Func<Task> function)
    {
        var gobj = new GameObject("TaskUpdator");
        var component = gobj.AddComponent<TaskUpdator>();
        component.function = function;
        return component;
    }

    /// <summary>
    /// Start
    /// </summary>
    private async Task Start()
    {
        await this.function();
        Destroy(this.gameObject);
    }
}
