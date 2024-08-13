import React, { useEffect, useState } from 'react';
import ChatRooms from './components/ChatRooms';
import ChatRoom from './components/ChatRoom';
import AuthPage from './components/AuthPage';
import { signalRService } from './signalRService';
import { RegionsEnum } from './api';

const App: React.FC = () => {
    const [username, setUsername] = useState<string>('');
    const [selectedRoomId, setSelectedRoomId] = useState<string | null>(null);
    const [selectedRegion, setSelectedRegion] = useState<RegionsEnum>(RegionsEnum.WORLD);
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
                <ChatRoom roomId={selectedRoomId} username={username} region={selectedRegion} onLeaveRoom={() => setSelectedRoomId(null)} />
            ) : (
                <ChatRooms username={username} onSelectRoom={setSelectedRoomId} onLogout={handleLogout} setSelectedRegion={setSelectedRegion}/>
            )}
        </div>
    );
};

export default App;