# AStarFindPath
길찾기 

목적 : 그냥 길찾기 알고리즘 나도 만들어 보고 싶어서??

- 기본 지도
![image](https://user-images.githubusercontent.com/116536524/198414014-b232b71c-3361-416f-b4dd-b74f3fbc0543.png)

- 구현
![image](https://user-images.githubusercontent.com/116536524/198414056-afc34a45-2e41-4878-b35a-bd4b6460ddee.png)

- 길찾기 결과
![image](https://user-images.githubusercontent.com/116536524/198414093-6b078bf4-c284-4ff2-bcdd-5308fd8f4185.png)


- 참고 정보 <br />
https://www.redblobgames.com/pathfinding/a-star/introduction.html <br />
https://qiao.github.io/PathFinding.js/visual/ <br />
http://itmining.tistory.com/66 <br />
http://blog.naver.com/PostView.nhn?blogId=denoil&logNo=221014139704 ( G값 계산에 대한 설명 )  <br />

- 코딩순서( 대략적인 )
 1. 맵( 2차원 배열 )을 만든다. 
 2. 시작위치, 종료위치를 셀 지정.
 3. 벽위치를 셀 지정
 4. 경로 탐색!

- == 경로탐색 방법 코딩순서 ==

 셀탐색( 대상셀 )
      isSearching() 
      {
 8방 셀 탐색<br />
![image](https://user-images.githubusercontent.com/116536524/198414380-2c584fae-858a-48f6-9e50-dae1d110b0f3.png)

 지정셀 = GetCell( x, y );
 지정셀.이전셀 = 대상셀
 *벽이면 x, 탐색된셀이면 x
 아니면 오픈셀 등록
 계산.
 8방향 모드 끝나면 목록 중 F값이 낮은 목록 검색
 셀탐색( 검색된 셀 );
 

- 만들어낸 코드순서 정리.
```
    . 자료목록
        - 검색대상목록 : Openeds
        - 제외대상목록 : Closeds

    . 맵을 생성
    . 시작점, 도착점 위치 셀정보를 취득 ( 고정값으로 맵을 만들때 지정하거나 직접 이동시킨다. )
    . 탐색!
        . 검색대상목록 추가 : 시작위치셀
        . while 1.도착점을 아직 못찾았다. 검색대상 셀목록이 있다.
            . 검색대상목록 중 F값이 제일 낮은 목록 취득
            . foreach 검색대상셀(A) in 취득된 검색대상목록
                . 검색대상셀에 인접한 셀 목록 취득
                . foreach 인접한셀(B) in 취득된 인접한 셀목록
                    . if 인접한셀 벽인가?,   제외대상목록인가? 
                            continue;
                    . if 검색대상목록에 있는 셀인가?
                            x
                    . else 
                            검색대상목록에 추가
                            인접한셀(B).이전노드 = 취득된 검색대상셀(A)
                            인접한셀(B).계산()
                                : F, G, H계산
                                : 이전노드 위치를 가리키는 방향계산    
``` 
