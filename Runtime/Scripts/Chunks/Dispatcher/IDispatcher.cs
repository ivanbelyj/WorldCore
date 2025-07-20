using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IDispatcher
{
    /// <summary>
    /// Планирует код для выполнения в главном потоке
    /// </summary>
    Task Execute(Action action);
    
    Task<T> Execute<T>(Func<T> func);
}
