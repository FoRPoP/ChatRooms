import React, { useState, useEffect, useRef } from 'react';
import { ChatApi } from '../api/apis/chat-api';
import { ChatData, Message, RegionsEnum } from '../api';
import { Label, PrimaryButton, Stack, TextField } from '@fluentui/react';
import { signalRService } from '../signalRService';
import axiosInstance from '../axiosConfig';

const ChatRoom: React.FC<{ roomId: string, username: string, region: RegionsEnum, onLeaveRoom: () => void }> = ({ roomId, username, region, onLeaveRoom }) => {
    const [messages, setMessages] = useState<Message[]>([]);
    const [newMessageText, setNewMessageText] = useState<string>('');
    const [chatData, setChatData] = useState<ChatData>();
    const [isAtBottom, setIsAtBottom] = useState(true);

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const chatContainerRef = useRef<HTMLDivElement>(null);

    const chatApi = new ChatApi(undefined, '', axiosInstance);

    useEffect(() => {
        const joinRoom = async () => {
            const connectionId = signalRService.getConnectionId();
            if (connectionId) {
                chatApi.chatJoinChatRoomPost(roomId, connectionId, region).then(response => {
                    setMessages(response.data.messages!);
                    setChatData(response.data.chatData!);
                    signalRService.joinGroup(roomId);
                });
            }
        };

        joinRoom();

        const handleReceiveMessage = (message: Message) => {
            if (message.text === 'Room is closing' && message.username === 'system') {
                onLeaveRoom();
            } else {
                setMessages(prevMessages => [...prevMessages, message]);
            }
        };

        signalRService.getConnection()?.on('ReceiveMessage', handleReceiveMessage);

        return () => {
            const leaveRoom = async () => {
                const connectionId = signalRService.getConnectionId();
                if (connectionId) {
                    chatApi.chatLeaveChatRoomPost(roomId, connectionId, chatData?.region)
                        .then(() => {
                            signalRService.leaveGroup(roomId);
                        });
                }
            };

            leaveRoom();

            signalRService.getConnection()?.off('ReceiveMessage', handleReceiveMessage);
        };
    }, [roomId, username]);

    useEffect(() => {
        if (isAtBottom) {
            messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
        }
    }, [messages, isAtBottom]);

    const handleScroll = () => {
        if (chatContainerRef.current) {
            const { scrollTop, scrollHeight, clientHeight } = chatContainerRef.current;
            setIsAtBottom(scrollTop + clientHeight >= scrollHeight - 10);
        }
    };

    const handleDeleteRoom = () => {
        const systemMessage: Message = { chatRoomId: roomId, username: 'system', text: 'Room is closing' };
        chatApi.chatSendMessagePost(systemMessage, roomId, region)
            .then(() => {
                chatApi.chatDeleteChatRoomDelete(roomId, region)
                    .catch(error => { console.error('There was an error deleting the chat room!', error); });
            })
    };

    const sendMessage = () => {
        if (newMessageText.trim()) {
            const message: Message = { chatRoomId: roomId, username: username, text: newMessageText };
            chatApi.chatSendMessagePost(message, roomId, region)
                .then(() => { setNewMessageText(''); })
                .catch(error => { console.error('There was an error sending the message!', error); });
        }
    };

    return (
        <Stack 
            tokens={{ childrenGap: 15 }} 
            styles={{ 
                root: { 
                    width: '100%', 
                    maxWidth: '800px', 
                    margin: '20px auto', 
                    padding: 20,
                    border: '1px solid #ccc',
                    borderRadius: 5,
                    boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                    backgroundColor: '#fff',
                    height: '80vh',
                    display: 'flex',
                    flexDirection: 'column'
                } 
            }}
        >
            <Stack horizontalAlign="center">
                <h2>{chatData?.name}</h2>
                <Label>Owner: {chatData?.ownerUsername}</Label>
            </Stack>
            <div 
                style={{ 
                    flex: 1,
                    overflowY: 'auto',
                    padding: '10px',
                    border: '1px solid #ccc',
                    borderRadius: 5,
                    boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                    backgroundColor: '#f9f9f9'
                }}
                onScroll={handleScroll}
                ref={chatContainerRef}
            >
                <Stack tokens={{ childrenGap: 10 }}>
                    {messages.map((msg, index) => (
                        <Stack 
                            key={index} 
                            horizontal 
                            horizontalAlign={msg.username === username ? 'end' : 'start'}
                            styles={{ 
                                root: { 
                                    marginBottom: '10px',
                                    textAlign: msg.username === username ? 'right' : 'left'
                                } 
                            }}
                        >
                            <Stack.Item 
                                styles={{ 
                                    root: { 
                                        maxWidth: '70%',
                                        padding: '10px',
                                        border: '1px solid #ccc',
                                        borderRadius: 5,
                                        boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                                        backgroundColor: msg.username === username ? '#e0f7fa' : '#fff'
                                    } 
                                }}
                            >
                                <Label styles={{ root: { fontWeight: 'bold' } }}>{msg.username}</Label>
                                <div>{msg.text}</div>
                                <Label styles={{ root: { fontSize: 'small', color: '#888' } }}>{new Date(msg.dateTimeSent!).toLocaleString()}</Label>
                            </Stack.Item>
                        </Stack>
                    ))}
                    <div ref={messagesEndRef} />
                </Stack>
            </div>
            <Stack horizontal tokens={{ childrenGap: 10 }} styles={{ root: { marginTop: '10px' } }}>
                <TextField 
                    placeholder="Type a message..." 
                    value={newMessageText} 
                    onChange={(e, value) => setNewMessageText(value || '')} 
                    styles={{ root: { flex: 1 } }}
                />
                <PrimaryButton text="Send" onClick={sendMessage} />
            </Stack>
            <Stack horizontal tokens={{ childrenGap: 10 }} styles={{ root: { marginTop: '10px' } }}>
                <PrimaryButton text="Leave Chat" onClick={onLeaveRoom} />
                {username.toLowerCase() === chatData?.ownerUsername!.toLowerCase() && <PrimaryButton text="Delete Chat" onClick={handleDeleteRoom} />}
            </Stack>
        </Stack>
    );
};

export default ChatRoom;