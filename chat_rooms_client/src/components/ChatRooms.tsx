import React, { useState, useEffect } from 'react';
import { Label, PrimaryButton, Stack, TextField } from '@fluentui/react';
import { ChatApi } from '../api/apis/chat-api';
import { ChatData } from '../api/models/chat-data';
import CreateChatRoomModal from './CreateChatRoomModal';

const ChatRooms: React.FC<{ username: string, onSelectRoom: (roomId: string, roomName: string) => void, onLogout: () => void}> = ({ username, onSelectRoom, onLogout }) => {
    const [chatRooms, setChatRooms] = useState<{ [key: string]: ChatData; }>({});
    const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
    const [filterText, setFilterText] = useState<string>('');
    const [expandedRoom, setExpandedRoom] = useState<string | null>(null);

    const chatApi = new ChatApi();

    useEffect(() => {
        refreshChatRooms();
    }, []);

    const handleCreateChatRoom = (roomName: string, roomDescription: string) => {
        chatApi.chatCreateChatRoomPost(username, roomName, roomDescription)
            .then(_ => {
                chatApi.chatGetChatRoomsGet()
                    .then(response => {
                        (setChatRooms(response.data))
                        setIsModalOpen(false);
                    });
            })
            .catch(error => { console.error('There was an error creating the chat room!', error); });
    };

    const refreshChatRooms = () => {
        chatApi.chatGetChatRoomsGet()
            .then(response => { setChatRooms(response.data); })
            .catch(error => { console.error('There was an error fetching chat rooms!', error); });
    };

    const filteredChatRooms = Object.keys(chatRooms).filter(key => 
        chatRooms[key].name?.toLowerCase().includes(filterText.toLowerCase())
    );

    return (
        <Stack 
            tokens={{ childrenGap: 15 }} 
            styles={{ 
                root: { 
                    width: 300, 
                    margin: '20px auto 0 auto', 
                    padding: 20,
                    border: '1px solid #ccc',
                    borderRadius: 5,
                    boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                    backgroundColor: '#fff'
                    } 
                }}
            >
            <h2>Available Chat Rooms</h2>
            <TextField
                placeholder={'Filter Chat Rooms....'}
                value={filterText}
                onChange={(e, value) => setFilterText(value || '')}
            />
            <Stack 
                tokens={{ childrenGap: 10 }}
                styles={{ 
                    root: { 
                        maxHeight: '800px',
                        overflowY: 'auto',
                        cursor: 'pointer', 
                        marginBottom: '10px', 
                        textAlign: 'center',
                        padding: '10px',
                        border: '1px solid #ccc',
                        borderRadius: 5,
                        boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                        backgroundColor: '#f9f9f9'
                    } 
                }}
            >
                {filteredChatRooms.map(key => (
                    <Stack.Item 
                        key={key} 
                        onClick={() => setExpandedRoom(expandedRoom === key ? null : key)} 
                        styles={{ 
                            root: { 
                                cursor: 'pointer', 
                                marginBottom: '10px', 
                                textAlign: 'center',
                                padding: '10px',
                                border: '1px solid #ccc',
                                borderRadius: 5,
                                boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                                backgroundColor: '#f9f9f9'
                            } 
                        }}
                    >
                        <strong>{chatRooms[key].name}</strong>
                        {expandedRoom === key && (
                            <Stack 
                                tokens={{ childrenGap: 10 }} 
                                horizontalAlign="center" 
                                styles={{ 
                                    root: { 
                                        marginTop: '10px',
                                        padding: '10px',
                                        border: '1px solid #ccc',
                                        borderRadius: 5,
                                        boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                                        backgroundColor: '#fff'
                                    } 
                                }}
                            >
                                <Label>Description: {chatRooms[key].description}</Label>
                                <Stack tokens={{ childrenGap: 5 }} horizontalAlign="center">
                                    <Label>Active Chatters: {chatRooms[key].activeChatters?.length || 0}</Label>
                                    <PrimaryButton onClick={() => onSelectRoom(key, chatRooms[key].name!)} text='Join Room' />
                                </Stack>
                            </Stack>
                        )}
                    </Stack.Item>
                ))}
            </Stack>
            <PrimaryButton onClick={() => setIsModalOpen(true)} text='Create Chat Room' />
            <CreateChatRoomModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                onCreate={handleCreateChatRoom}
            />
            <PrimaryButton onClick={() => refreshChatRooms()} text="Refresh" />
            <PrimaryButton onClick={onLogout} text="Logout" />
        </Stack>
    );
};

export default ChatRooms;