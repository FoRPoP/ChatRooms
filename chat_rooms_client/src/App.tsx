import React, { useEffect, useState } from 'react';
import ChatRooms from './components/ChatRooms';
import ChatRoom from './components/ChatRoom';
import AuthPage from './components/AuthPage';
import { signalRService } from './signalRService';

const App: React.FC = () => {
    const [username, setUsername] = useState<string>('');
    const [selectedRoomId, setSelectedRoomId] = useState<string | null>(null);
    const [selectedRoomName, setSelectedRoomName] = useState<string>('');
    const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);

    useEffect(() => {
        if (isLoggedIn) {
            signalRService.startConnection('http://localhost:8314/hub');
        }
    }, [isLoggedIn]);

    const handleLogin = (username: string) => {
        setUsername(username);
        setIsLoggedIn(true);
    }

    const handleLogout = () => {
        setUsername('');
        setIsLoggedIn(false);
    }
 
    if (!isLoggedIn) {
        return <AuthPage onLogin={handleLogin}/>;
    }

    return (
        <div>
            {selectedRoomId ? (
                <ChatRoom roomId={selectedRoomId} username={username} onLeaveRoom={() => setSelectedRoomId(null)} />
            ) : (
                <ChatRooms username={username} onSelectRoom={setSelectedRoomId} onLogout={handleLogout}/>
            )}
        </div>
    );
};

export default App;