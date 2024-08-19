import React, { useState, useEffect } from 'react';
import { Label, PrimaryButton, Stack, TextField, Checkbox, Dropdown, IDropdownOption } from '@fluentui/react';
import { Filter28Regular, Star24Filled, Star24Regular } from '@fluentui/react-icons';
import { ChatApi } from '../api/apis/chat-api';
import { ChatData } from '../api/models/chat-data';
import CreateChatRoomModal from './CreateChatRoomModal';
import axiosInstance from '../axiosConfig';
import { UserInfo } from '../api/models/user-info';
import { RegionsEnum } from '../api';

const ChatRooms: React.FC<{ username: string, region: RegionsEnum, onSelectRoom: (roomId: string, roomName: string) => void, onLogout: () => void, setRegion: (region: RegionsEnum) => void}> = ({ username, region, onSelectRoom, onLogout, setRegion }) => {
    const [chatRooms, setChatRooms] = useState<{ [key: string]: ChatData; }>({});
    const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
    const [filterText, setFilterText] = useState<string>('');
    const [expandedRoom, setExpandedRoom] = useState<string | null>(null);
    const [userInfo, setUserInfo] = useState<UserInfo>({});
    const [activeChattersFilter, setActiveChattersFilter] = useState<number | null>(null);
    const [messagesSentFilter, setMessagesSentFilter] = useState<number | null>(null);
    const [creatorNameFilter, setCreatorNameFilter] = useState<string>('');
    const [filterFavourited, setFilterFavourited] = useState<boolean>(false);
    const [filterCreated, setFilterCreated] = useState<boolean>(false);
    const [isDropdownVisible, setIsDropdownVisible] = useState<boolean>(false);

    const chatApi = new ChatApi(undefined, '', axiosInstance);

    useEffect(() => {
        refreshChatRooms(region);
        setRegion(region);
        chatApi.chatGetUserInfoGet(region)
            .then(response => setUserInfo(response.data))
            .catch(error => { console.error('There was an error fetching user info!', error); });
    }, [chatApi, region, setRegion]);

    const handleCreateChatRoom = (roomName: string, roomDescription: string) => {
        chatApi.chatCreateChatRoomPost(roomName, roomDescription, region)
            .then(_ => {
                chatApi.chatGetChatRoomsGet(region)
                    .then(response => {
                        setChatRooms(response.data);
                        setIsModalOpen(false);
                    });
            })
            .catch(error => { console.error('There was an error creating the chat room!', error); });
    };

    const refreshChatRooms = (selectedRegion: RegionsEnum) => {
        chatApi.chatGetChatRoomsGet(selectedRegion)
            .then(response => { setChatRooms(response.data); })
            .catch(error => { console.error('There was an error fetching chat rooms!', error); });
    };

    const handleFavouriteToggle = (roomId: string) => {
        chatApi.chatFavouriteChatRoomPost(roomId, region)
            .then(() => {
                chatApi.chatGetUserInfoGet(region)
                    .then(response => setUserInfo(response.data))
                    .catch(error => { console.error('There was an error fetching user info!', error); });
            })
            .catch(error => { console.error('There was an error favouriting/unfavouriting the chat room!', error); });
    };

    const filteredChatRooms = Object.keys(chatRooms).filter(key => {
        const room = chatRooms[key];
        const matchesFilterText = room.name?.toLowerCase().includes(filterText.toLowerCase());
        const matchesActiveChatters = activeChattersFilter === null || (room.activeChatters?.length || 0) >= activeChattersFilter;
        const matchesMessagesSent = messagesSentFilter === null || (room.totalMessages || 0) >= messagesSentFilter;
        const matchesCreatorName = creatorNameFilter === '' || room.ownerUsername === creatorNameFilter;
        const matchesFavourited = !filterFavourited || userInfo.favouritedChatsIds!.includes(key);
        const matchesCreated = !filterCreated || userInfo.createdChatsIds!.includes(key);

        return matchesFilterText && matchesActiveChatters && matchesMessagesSent && matchesCreatorName && matchesFavourited && matchesCreated;
    });

    const resetFilters = () => {
        setFilterText('');
        setActiveChattersFilter(null);
        setMessagesSentFilter(null);
        setCreatorNameFilter('');
        setFilterFavourited(false);
        setFilterCreated(false);
    };

    const regionOptions: IDropdownOption[] = Object.keys(RegionsEnum).map(key => ({
        key: key,
        text: key
    }));

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
            <Dropdown
                placeholder="Select a region"
                options={regionOptions}
                selectedKey={region}
                onChange={(e, option) => {
                    setRegion(option?.key as RegionsEnum);
                    setRegion(option?.key as RegionsEnum);
                    refreshChatRooms(option?.key as RegionsEnum);
                    chatApi.chatGetUserInfoGet(option?.key as RegionsEnum)
                }}
                styles={{ dropdown: { width: 200 } }}
            />
            <h2>Available Chat Rooms</h2>
            <Stack horizontal tokens={{ childrenGap: 20 }}>
                <TextField
                    placeholder={'Filter Chat Rooms....'}
                    value={filterText}
                    onChange={(e, value) => setFilterText(value || '')}
                    styles={{ root: { width: 200 } }}
                />
                <Filter28Regular onClick={() => setIsDropdownVisible(!isDropdownVisible)}/>
            </Stack>
            {isDropdownVisible && (
                <Stack tokens={{ childrenGap: 10 }}>
                    <TextField
                        label="Filter by creator name"
                        value={creatorNameFilter}
                        onChange={(e, value) => setCreatorNameFilter(value || '')}
                    />
                    <TextField
                        label="Filter by active chatters"
                        type="number"
                        value={activeChattersFilter?.toString() || ''}
                        onChange={(e, value) => setActiveChattersFilter(value ? parseInt(value) : null)}
                    />
                    <TextField
                        label="Filter by messages sent"
                        type="number"
                        value={messagesSentFilter?.toString() || ''}
                        onChange={(e, value) => setMessagesSentFilter(value ? parseInt(value) : null)}
                    />
                    <PrimaryButton text="Reset Filters" onClick={resetFilters} />
                </Stack>
            )}
            <Stack horizontal horizontalAlign="center" tokens={{ childrenGap: 10 }}>
                <Checkbox
                    label="Favourited"
                    checked={filterFavourited}
                    onChange={(e, checked) => setFilterFavourited(!!checked)}
                />
                <Checkbox
                    label="Created"
                    checked={filterCreated}
                    onChange={(e, checked) => setFilterCreated(!!checked)}
                />
            </Stack>
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
                            <Stack tokens={{ childrenGap: 5 }} horizontalAlign="center">
                                <Label>Description: {chatRooms[key].description}</Label>
                                <Label>Active Chatters: {chatRooms[key].activeChatters?.length || 0}</Label>
                                <Label>Total Messages: {chatRooms[key].totalMessages || 0}</Label>
                                <PrimaryButton onClick={() => onSelectRoom(key, chatRooms[key].name!)} text='Join Room' />
                                <span 
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        handleFavouriteToggle(key);
                                    }}
                                    style={{ cursor: 'pointer' }}
                                >
                                    {userInfo.favouritedChatsIds!.includes(key) ? <Star24Filled /> : <Star24Regular />}
                                </span>
                            </Stack>
                        </Stack>
                    )}
                </Stack.Item>
            ))}
            <PrimaryButton onClick={() => setIsModalOpen(true)} text='Create Chat Room' />
            <CreateChatRoomModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                onCreate={handleCreateChatRoom}
            />
            <PrimaryButton onClick={() => refreshChatRooms(region)} text="Refresh" />
            <PrimaryButton onClick={onLogout} text="Logout" />
        </Stack>
    );
};

export default ChatRooms;