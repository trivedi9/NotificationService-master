/*
 *  File Name : TTSVoice.cs
 *  Author : Vasu Ravuri
 *  @ PCC Technology Group LLC
 *  Created Date : 01/03/2011
 */
internal class TTSVoice
{

    private ITTSVoiceEvents m_EventSink;
    private short m_Index;
    private SpeechLib.SpVoice withEventsField_speechVoice;
    private SpeechLib.ISpeechMMSysAudio speechMMSysAudioOut;

    public ITTSVoiceEvents EventSink
    {
        get { return m_EventSink; }
        set { m_EventSink = value; }
    }
    
    public SpeechLib.SpVoice SPVoice
    {
        get { return speechVoice; }
    }

    public SpeechLib.ISpeechMMSysAudio MMSysAudioOut
    {
        get { return speechMMSysAudioOut; }
    }

    /// <summary>
    /// Converts text to speech.
    /// </summary>
    private SpeechLib.SpVoice speechVoice
    {
        get { return withEventsField_speechVoice; }
        set
        {
            if (withEventsField_speechVoice != null)
            {
                withEventsField_speechVoice.EndStream -= speechVoice_EndStream;
                withEventsField_speechVoice.StartStream -= speechVoice_StartStream;
            }
            withEventsField_speechVoice = value;
            if (withEventsField_speechVoice != null)
            {
                withEventsField_speechVoice.EndStream += speechVoice_EndStream;
                withEventsField_speechVoice.StartStream += speechVoice_StartStream;
            }
        }
    }

    /// <summary>
    /// Initializes the class
    /// </summary>
    private void ClassInit()
    {
        m_Index = 0;
        m_EventSink = null;

        speechVoice = new SpeechLib.SpVoice();
        speechVoice.EventInterests = SpeechLib.SpeechVoiceEvents.SVEEndInputStream | SpeechLib.SpeechVoiceEvents.SVEStartInputStream;

        speechMMSysAudioOut = new SpeechLib.SpMMAudioOut();
    }
    public TTSVoice()
        : base()
    {
        ClassInit();
    }
    
    /// <summary>
    /// Speech event.
    /// </summary>
    /// <param name="StreamNumber"></param>
    /// <param name="StreamPosition"></param>
    private void speechVoice_EndStream(int StreamNumber, object StreamPosition)
    {
        if (m_EventSink == null)
            return;
        m_EventSink.EndStream(ref m_Index, StreamNumber, StreamPosition);
    }

    /// <summary>
    /// Speech event.
    /// </summary>
    /// <param name="StreamNumber"></param>
    /// <param name="StreamPosition"></param>
    private void speechVoice_StartStream(int StreamNumber, object StreamPosition)
    {
        if (m_EventSink == null)
            return;
        m_EventSink.StartStream(ref m_Index, StreamNumber, StreamPosition);
    }

    

}
