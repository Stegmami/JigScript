// Author: Kelly Rey Wilson kelly@MolecularJig.com
//
// Copyright (c) 2014, NightPen, LLC and MolecularJig
//
// All rights reserved.
//
// when() statement Patent Pending
//
// While the source to JigScript is copyrighted, any JigScript
// add on function libraries you create or any JigScript script
// files you create are yours to do with as you please! If you
// develop a really cool game or function library, we would
// love to see it. You can contact us at MolecularJig.com
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NightPen.JigScript
{
    public class SoundFunctions : JigExtension
    {
        internal class SoundInfo
        {
            public AudioClip clip;
            public AudioSource source;
            public string name;
            public bool loop;
            public float volume;
            public float pitch;
            public bool changed;
            
            public SoundInfo()
            {
                this.clip = null;
                this.source = null;
                this.name = string.Empty;
                this.loop = false;
                this.volume = 1.0f;
                this.pitch = 1.0f;
                this.changed = false;
            }
            
            public SoundInfo( AudioClip clip, string name )
            {
                this.clip = clip;
                this.source = null;
                this.name = name;
                this.loop = false;
                this.volume = 1.0f;
                this.pitch = 1.0f;
                this.changed = false;
            }
        };
        
        private const int maxAudioSources = 10; //max audio sources to allow. Since these are reused as they become available, this should be more than enough.
        List<SoundInfo>sounds = new List<SoundInfo>();
    
        IEnumerator CreateFunction( List<Value> values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("sound = Sounds.Create(sound file);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.String);
                
                AudioClip audioClip = (AudioClip)Resources.Load(values[0].S);
                if ( audioClip == null )
                {
                    Debug.LogError("Sound file " + values[0].S + " does not exist.");
                }
                else
                {
                    AudioListener audioListener = Camera.main.gameObject.GetComponent<AudioListener>();
                    if ( audioListener == null )
                    {
                        audioListener = Camera.main.gameObject.AddComponent<AudioListener>();
                    }
                    Value v = new Value(sounds.Count, "sound_file_" + sounds.Count);
                    v.sound = sounds.Count;
                    values.Add(v);
                    
                    sounds.Add(new SoundInfo(audioClip, "sound_file_" + sounds.Count));
                }
            }
            
            yield return 0;
        }
    
        IEnumerator DestroyFunction( List<Value> values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("Sounds.Destroy(sound);");
            }
            else
            {
                if ( values[0].sound > 0 && values[0].sound < sounds.Count )
                {
                    if ( sounds[values[0].sound].source != null && sounds[values[0].sound].source.clip != null )
                    {
                        sounds[values[0].sound].source.Stop();
                        sounds[values[0].sound].source.clip = null;
                    }
                }
            }
            
            yield return 0;
        }
        
        public void Clear()
        {
            foreach(SoundInfo si in sounds)
            {
                if ( si.source != null )
                {
                    si.source.Stop();
                    si.source.clip = null;
                }
            }
            sounds.Clear();
            sounds.Add(new SoundInfo());
        }
        
        private AudioSource GetAvailableAudioSource()
        {
            AudioSource source = null;
            AudioSource[] audioSources = Camera.main.GetComponents<AudioSource>();
    
            foreach( AudioSource audioSource in audioSources )
            {
                if ( audioSource.loop == false && audioSource.isPlaying == false )
                {
                    source = audioSource;
                    break;
                }
            }
            if ( source == null )
            {
                if ( audioSources.Length < maxAudioSources )
                {
                    source = Camera.main.gameObject.AddComponent<AudioSource>();
                }
            }
            return source;
        }
    
        private Value GetPlay( Value v )
        {
            v.B = false;
            v.T = Value.ValueType.Bool;
            
            if ( v.sound > 0 && v.sound < sounds.Count && sounds[v.sound].source != null )
            {
                v.B = sounds[v.sound].source.isPlaying;
            }
            return v;
        }
        
        private void SetPlay( Value dest, int arrayIndex, Value source )
        {
            List<Value> va = Variables.Expand(dest, false);
            
            source.ConvertTo(Value.ValueType.Bool);
            for(int ii=1; ii<va.Count; ++ii)
            {
                int index = va[ii].sound;
                
                if ( index > 0 && index < sounds.Count )
                {
                    if ( source.B )
                    {
                        sounds[index].source = GetAvailableAudioSource();
                        if ( sounds[index].source == null )
                        {
                            Debug.LogError("No audio source available to play sound.");
                            break;
                        }
                        else
                        {
                            sounds[index].source.clip = sounds[index].clip;
                            sounds[index].source.loop = sounds[index].loop;
                            sounds[index].source.pitch = sounds[index].pitch;
                            sounds[index].source.volume = sounds[index].volume;
                            sounds[index].source.Play();
                            CPU.whensNeeded = true;
                        }
                    }
                    else
                    {
                        if ( sounds[index].source != null )
                        {
                            sounds[index].source.Stop();
                            if ( sounds[index].source.clip != null )
                            {
                                sounds[index].source.clip = null;
                                CPU.whensNeeded = true;
                            }
                        }
                    }
                }
            }
        }
        
        private Value GetVolume( Value v )
        {
            if ( v.sound > 0 && v.sound < sounds.Count )
            {
                v.T = Value.ValueType.Float;
                v.F = sounds[v.sound].volume;
            }
            return v;
        }
        
        private void SetVolume( Value dest, int arrayIndex, Value source )
        {
            List<Value> va = Variables.Expand(dest, false);
            
            source.ConvertTo(Value.ValueType.Float);
            
            for(int ii=1; ii<va.Count; ++ii)
            {
                if ( va[ii].sound > 0 && va[ii].sound < sounds.Count )
                {
                    sounds[va[ii].sound].volume = source.F;
                    CPU.whensNeeded = true;
                }
            }
        }
        
        private Value GetLoop( Value v )
        {
            if ( v.sound > 0 && v.sound < sounds.Count )
            {
                v.T = Value.ValueType.Bool;
                v.B = sounds[v.sound].loop;
            }
            return v;
        }
        
        private void SetLoop( Value dest, int arrayIndex, Value source )
        {
            List<Value> va = Variables.Expand(dest, false);
            
            source.ConvertTo(Value.ValueType.Bool);
            
            for(int ii=1; ii<va.Count; ++ii)
            {
                if ( va[ii].sound > 0 && va[ii].sound < sounds.Count )
                {
                    sounds[va[ii].sound].loop = source.B;
                    CPU.whensNeeded = true;
                }
            }
        }
        
        private Value GetPitch( Value v )
        {
            if ( v.sound > 0 && v.sound < sounds.Count )
            {
                v.T = Value.ValueType.Float;
                v.F = sounds[v.sound].pitch;
            }
            return v;
        }
        
        private void SetPitch( Value dest, int arrayIndex, Value source )
        {
            List<Value> va = Variables.Expand(dest, false);
            
            source.ConvertTo(Value.ValueType.Float);
            
            for(int ii=1; ii<va.Count; ++ii)
            {
                if ( va[ii].sound > 0 && va[ii].sound < sounds.Count )
                {
                    sounds[va[ii].sound].pitch = source.F;
                    CPU.whensNeeded = true;
                }
            }
        }
        
        public override void Initialize( JigCompiler compiler )
        {
            sounds.Add(new SoundInfo());
            compiler.AddFunction("Sounds.Create", CreateFunction);
            compiler.AddFunction("Sounds.Destroy", DestroyFunction);
            
            Variables.CreateCustomProperty("play", GetPlay, SetPlay);
            Variables.CreateCustomProperty("volume", GetVolume, SetVolume);
            Variables.CreateCustomProperty("loop", GetLoop, SetLoop);
            Variables.CreateCustomProperty("pitch", GetPitch, SetPitch);
        }
    }
}