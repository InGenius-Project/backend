﻿namespace IngBackend.Interfaces.Repository;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}
