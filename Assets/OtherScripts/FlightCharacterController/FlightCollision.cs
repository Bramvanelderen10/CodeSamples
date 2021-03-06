﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles what happens when the player collides with another object
/// We do this because we do not want the default behaviour of unity collisions
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FlightCollision : MonoBehaviour
{
    [SerializeField] private List<string> _ignoreTags = new List<string> {"Player"};
    [SerializeField] private float _velocityBumpLimit; //Determines from which velocity a bump is actually a bump
    [SerializeField] private float _knockbackVelocity = 20f; //The velocity that is applied to the target in the opposite direction of the bump direction
    [SerializeField] private float _distanceInterval = 3f; //The amount of distant the target has to travel until a next bump can happen
    [SerializeField] private float _checkDistanceInterval = .1f;

    private Rigidbody _rb;
    private float _distanceCounter = 0f;
    private Vector3 _lastLocation;
    private Action _bumpListeners;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _lastLocation = transform.position;

        StartCoroutine(CheckDistance()); //Start checking
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_ignoreTags.Contains(collision.gameObject.tag) && _distanceCounter == 0) //Filter unneeded collisions
            return;

        if (collision.relativeVelocity.magnitude > _velocityBumpLimit) //If the collision is high speed do bump
        {
            _distanceCounter = _distanceInterval; //Reset distance counter
            _rb.angularVelocity = Vector3.zero; //Reset angular velocity since we don't want that
            var point = collision.contacts[0].point;
            _rb.velocity = (transform.position - (point)).normalized*_knockbackVelocity; //Velocity in the opposite direction of the collision point
            _bumpListeners(); //Send event to the listeners
        }
    }

    /// <summary>
    /// Checks for distance every interval. 
    /// This is done so that the player can't keep bouncing around after 1 collision
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckDistance()
    {
        while (true)
        {
            var dist = Vector3.Distance(_lastLocation, transform.position);
            if (dist > .5f)
                _distanceCounter -= dist;
            if (_distanceCounter < 0)
                _distanceCounter = 0;
            _lastLocation = transform.position;
            yield return new WaitForSeconds(_checkDistanceInterval);
        }
    }

    /// <summary>
    /// Add a listener to the bump event
    /// </summary>
    /// <param name="listener"></param>
    public void AddBumpListener(Action listener)
    {
        if (_bumpListeners == null)
            _bumpListeners = listener;
        _bumpListeners += listener;
    }

    /// <summary>
    /// Remove a listener from the bump event
    /// </summary>
    /// <param name="listener"></param>
    public void RemoveBumpListener(Action listener)
    {
        if (_bumpListeners != null)
            _bumpListeners -= listener;
    }
}